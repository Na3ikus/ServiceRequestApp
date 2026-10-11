using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Profile;
using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Application.Services.WorkLogs;
using ServiceDeskSystem.Components.Pages.Profile;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Pages;

[TestFixture]
public class ProfilePageTests
{
    private ComponentTestHost _host;
    private Mock<IProfileService> _profile;
    private Mock<ITicketService> _tickets;
    private Mock<ITicketStatisticsService> _stats;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();

        _profile = new Mock<IProfileService>();
        _profile.Setup(p => p.GetContactTypesAsync()).ReturnsAsync([new ContactTypeDto(1, "email")]);
        _profile.Setup(p => p.GetProfileAsync(It.IsAny<int>())).ReturnsAsync(new UserProfileDto(
            1, "admin", "Olena", "Koval", null, null, [new ContactInfoDto(5, "olena@example.com", true, 1, "email")]));

        _tickets = new Mock<ITicketService>();
        _tickets.Setup(t => t.GetUserTicketsAsync(It.IsAny<int>())).ReturnsAsync(
        [
            new Ticket { Id = 1, Title = "a", Status = TicketStatus.Open },
            new Ticket { Id = 2, Title = "b", Status = TicketStatus.Resolved },
            new Ticket { Id = 3, Title = "c", Status = TicketStatus.Closed },
        ]);
        _tickets.Setup(t => t.GetDeveloperTicketsAsync(It.IsAny<int>())).ReturnsAsync([new Ticket { Id = 4, Title = "d" }]);

        _stats = new Mock<ITicketStatisticsService>();
        _stats.Setup(s => s.GetDeveloperDashboardStatsAsync(It.IsAny<int>())).ReturnsAsync(new DeveloperDashboardStatsDto(6, 2, 3));
        var workLogs = new Mock<IWorkLogService>();
        workLogs.Setup(w => w.GetTotalTimeSpentForUserAsync(It.IsAny<int>())).ReturnsAsync(90);
        var activity = new Mock<IUserActivityService>();
        activity.Setup(a => a.GetDailyActivityAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync([]);

        _host.Context.Services.AddSingleton(_profile.Object);
        _host.Context.Services.AddSingleton(_tickets.Object);
        _host.Context.Services.AddSingleton(_stats.Object);
        _host.Context.Services.AddSingleton(workLogs.Object);
        _host.Context.Services.AddSingleton(activity.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private void SignIn(UserRole role) =>
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 1, Login = "admin", Role = role });

    private static IEnumerable<string> Numbers(IRenderedComponent<UserProfile> cut) =>
        cut.FindAll("[data-number]").Select(n => $"{n.QuerySelector("dt")!.TextContent}={n.QuerySelector("dd")!.TextContent}");

    [Test]
    public void Identity_card_follows_the_form_while_typing()
    {
        var cut = _host.Context.RenderComponent<UserProfile>();

        cut.Find("[data-full-name]").TextContent.Should().Be("Olena Koval");
        cut.Find("[data-identity]").TextContent.Should().Contain("@admin").And.Contain("olena@example.com");

        cut.Find("#profile-first").Input("Ola");

        cut.Find("[data-full-name]").TextContent.Should().Be("Ola Koval");
    }

    [Test]
    public void User_numbers_are_created_open_resolved()
    {
        SignIn(UserRole.User);

        var cut = _host.Context.RenderComponent<UserProfile>();

        Numbers(cut).Should().Equal("profile.stat.created=3", "profile.stat.open=1", "profile.stat.resolved=2");
    }

    [Test]
    public void Staff_numbers_include_hours_and_recent_assigned_tickets()
    {
        SignIn(UserRole.Developer);

        var cut = _host.Context.RenderComponent<UserProfile>();

        Numbers(cut).Should().Equal(
            "profile.stat.assigned=6", "profile.stat.inprogress=2", "profile.stat.completed=3", "profile.stat.hours=1.5");
        cut.Find("[data-ticket-row]").GetAttribute("href").Should().Be("/ticket/4");
    }

    [Test]
    public void Admin_gets_the_staff_numbers()
    {
        var cut = _host.Context.RenderComponent<UserProfile>();

        Numbers(cut).Should().HaveCount(4).And.Contain("profile.stat.assigned=6");
    }

    [Test]
    public void Failed_stats_drop_the_skeletons_but_keep_the_activity_map()
    {
        _stats.Setup(s => s.GetDeveloperDashboardStatsAsync(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException());

        var cut = _host.Context.RenderComponent<UserProfile>();

        cut.FindAll("[data-number]").Should().BeEmpty();
        cut.FindAll("[aria-busy=true]").Should().BeEmpty();
        cut.FindAll("[data-heatmap]").Should().ContainSingle();
        cut.Markup.Should().Contain("home.noRecentActivity");
    }

    [Test]
    public void Editing_after_save_clears_the_ok_mark()
    {
        _profile.Setup(p => p.UpdateProfileAsync(1, It.IsAny<UpdateProfileRequest>())).ReturnsAsync((true, (string?)null));
        var cut = _host.Context.RenderComponent<UserProfile>();
        cut.Find("form").Submit();

        cut.Find("#profile-last").Input("Kovalenko");

        cut.FindAll("[data-saved]").Should().BeEmpty();
    }

    [Test]
    public void Has_path_header_activity_map_and_no_gradients()
    {
        var cut = _host.Context.RenderComponent<UserProfile>();

        cut.Find("h1").TextContent.Should().Be("~/profile");
        cut.FindAll("[data-heatmap]").Should().ContainSingle();
        cut.Markup.Should().NotContain("gradient").And.NotContain("shadow-lg");
    }

    [Test]
    public void Empty_first_name_shows_alert_and_does_not_save()
    {
        var cut = _host.Context.RenderComponent<UserProfile>();

        cut.Find("#profile-first").Input(string.Empty);
        cut.Find("form").Submit();

        cut.Find("[role=alert]").TextContent.Should().Be("profile.validation.firstNameRequired");
        _profile.Verify(p => p.UpdateProfileAsync(It.IsAny<int>(), It.IsAny<UpdateProfileRequest>()), Times.Never);
    }

    [Test]
    public void Successful_save_shows_ok_status()
    {
        _profile.Setup(p => p.UpdateProfileAsync(1, It.IsAny<UpdateProfileRequest>())).ReturnsAsync((true, (string?)null));
        var cut = _host.Context.RenderComponent<UserProfile>();

        cut.Find("form").Submit();

        cut.Find("[data-saved]").TextContent.Should().Contain("[ok]");
    }

    [Test]
    public void Failed_save_keeps_the_form_and_shows_translated_error()
    {
        _profile.Setup(p => p.UpdateProfileAsync(1, It.IsAny<UpdateProfileRequest>())).ReturnsAsync((false, "Failed to update profile: boom"));
        var cut = _host.Context.RenderComponent<UserProfile>();

        cut.Find("form").Submit();

        cut.Find("[role=alert]").TextContent.Should().Be("profile.error.save");
        cut.FindAll("#profile-first").Should().ContainSingle();
    }
}
