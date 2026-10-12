using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Common.Models;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using HomePage = ServiceDeskSystem.Components.Pages.Index;

namespace ServiceDeskSystem.Tests.Frontend.Components.Pages;

[TestFixture]
public class HomePageTests
{
    private ComponentTestHost _host;
    private Mock<ITicketService> _tickets;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _tickets = new Mock<ITicketService>();
        _tickets.Setup(t => t.GetPagedTicketsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new PagedResult<Ticket> { Items = [Ticket(1, assessed: false), Ticket(2)] });
        _tickets.Setup(t => t.GetUserTicketsAsync(It.IsAny<int>())).ReturnsAsync([Ticket(3)]);

        var stats = new Mock<ITicketStatisticsService>();
        _host.Context.Services.AddSingleton(_tickets.Object);
        _host.Context.Services.AddSingleton(stats.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private static Ticket Ticket(int id, bool assessed = true) => new()
    {
        Id = id,
        Title = $"Ticket {id}",
        CreatedAt = new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
        IsPriorityAssessed = assessed,
    };

    private void SignIn(UserRole role) =>
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 7, Login = "olena", Role = role });

    [Test]
    public void Greets_with_blinking_cursor_and_has_no_hero()
    {
        var cut = _host.Context.RenderComponent<HomePage>();

        cut.Find("h1").TextContent.Should().StartWith("~").And.Contain("nav.home");
        cut.Find("h1 .sr-only").TextContent.Should().Be("nav.home");
        cut.Find("h2").TextContent.Should().Contain("home.welcomeBack admin");
        cut.Find("h2 .cursor-blink").TextContent.Should().Be("_");
        cut.Markup.Should().NotContain("page-hero").And.NotContain("gradient");
    }

    [Test]
    public void Staff_see_overview_triage_notice_and_triage_marks()
    {
        var cut = _host.Context.RenderComponent<HomePage>();

        cut.Markup.Should().Contain("# home.overview");
        cut.Find("[data-triage-notice] a").GetAttribute("href").Should().Be("tickets");
        cut.FindAll("[data-ticket-row]").Should().HaveCount(2);
        cut.FindAll("[data-triage]").Should().ContainSingle();
    }

    [Test]
    public void Developer_sees_assigned_tickets_and_triage_notice()
    {
        SignIn(UserRole.Developer);
        _tickets.Setup(t => t.GetDeveloperTicketsAsync(7)).ReturnsAsync([Ticket(9)]);

        var cut = _host.Context.RenderComponent<HomePage>();

        cut.Find("[data-ticket-row]").GetAttribute("href").Should().Be("/ticket/9");
        cut.FindAll("[data-triage-notice]").Should().ContainSingle();
        cut.FindAll("a[href='/tickets']").Should().HaveCount(2);
    }

    [Test]
    public void User_gets_own_tickets_links_and_no_overview()
    {
        SignIn(UserRole.User);

        var cut = _host.Context.RenderComponent<HomePage>();

        cut.Markup.Should().NotContain("# home.overview");
        cut.FindAll("[data-triage-notice]").Should().BeEmpty();
        cut.FindAll("a[href='/my-tickets']").Should().HaveCount(2);
        cut.Find("[data-ticket-row]").GetAttribute("href").Should().Be("/ticket/3");
    }

    [Test]
    public void Shows_empty_state_when_no_tickets()
    {
        SignIn(UserRole.User);
        _tickets.Setup(t => t.GetUserTicketsAsync(It.IsAny<int>())).ReturnsAsync([]);

        var cut = _host.Context.RenderComponent<HomePage>();

        cut.Markup.Should().Contain("home.noRecentActivity");
        cut.FindAll("[data-ticket-row]").Should().BeEmpty();
    }
}
