using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Components.Features.Kanban;
using ServiceDeskSystem.Components.Pages.Developer;
using ServiceDeskSystem.Components.Pages.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Tickets;

[TestFixture]
public class TicketListPagesTests
{
    private ComponentTestHost _host;
    private Mock<ITicketService> _tickets;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _tickets = new Mock<ITicketService>();
        List<Ticket> list =
        [
            new() { Id = 1, Title = "Printer jam", Description = "", Status = TicketStatus.Open, Author = new User { Login = "olena" } },
            new() { Id = 2, Title = "VPN down", Description = "", Status = TicketStatus.InProgress, Author = new User { Login = "ivan" }, DeveloperId = 7 },
        ];
        _tickets.Setup(t => t.GetUserTicketsAsync(It.IsAny<int>())).ReturnsAsync(list);
        _tickets.Setup(t => t.GetDeveloperTicketsAsync(It.IsAny<int>())).ReturnsAsync(list);
        _tickets.Setup(t => t.GetAllTicketsAsync()).ReturnsAsync(list);

        var stats = new Mock<ITicketStatisticsService>();
        stats.Setup(s => s.GetDeveloperDashboardStatsAsync(It.IsAny<int>())).ReturnsAsync(new DeveloperDashboardStatsDto(4, 1, 2));

        _host.Context.Services.AddSingleton(_tickets.Object);
        _host.Context.Services.AddSingleton(stats.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private void SignIn(UserRole role) =>
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 7, Login = "dev", Role = role });

    [Test]
    public void My_tickets_has_path_header_filters_and_no_hero()
    {
        SignIn(UserRole.User);

        var cut = _host.Context.RenderComponent<MyTickets>();

        cut.Find("h1").TextContent.Should().Be("~/my-tickets");
        cut.Markup.Should().NotContain("page-hero").And.NotContain("gradient");
        cut.Find("[data-counter]").TextContent.Should().Be("2 / 2");
        cut.FindAll("button[data-sort=Author]").Should().BeEmpty();
    }

    [Test]
    public void Searching_narrows_the_list_and_reset_restores_it()
    {
        SignIn(UserRole.User);
        var cut = _host.Context.RenderComponent<MyTickets>();

        cut.Find("[data-ticket-filters] input").Input("vpn");

        cut.Find("[data-counter]").TextContent.Should().Be("1 / 2");
        cut.FindAll("tr[data-ticket-row]").Should().ContainSingle();

        cut.Find("[data-reset]").Click();

        cut.Find("[data-counter]").TextContent.Should().Be("2 / 2");
        cut.FindAll("[data-reset]").Should().BeEmpty();
    }

    [Test]
    public void No_match_offers_to_reset_filters()
    {
        SignIn(UserRole.User);
        var cut = _host.Context.RenderComponent<MyTickets>();

        cut.Find("[data-ticket-filters] input").Input("nothing like this");

        cut.Markup.Should().Contain("tickets.nothingFound");
        cut.FindAll("tr[data-ticket-row]").Should().BeEmpty();
    }

    [Test]
    public void Developer_page_uses_mono_figures_without_assignee_column()
    {
        SignIn(UserRole.Developer);

        var cut = _host.Context.RenderComponent<DeveloperDashboard>();

        cut.Find("h1").TextContent.Should().Be("~/developer");
        cut.FindAll("[data-stat] p.text-40").Select(p => p.TextContent).Should().Equal("4", "1", "2");
        cut.FindAll("button[data-sort=Assignee]").Should().BeEmpty();
        cut.Markup.Should().NotContain("gradient");
    }

    [Test]
    public void Developer_page_switches_to_kanban_with_hash_column_titles()
    {
        SignIn(UserRole.Developer);
        var cut = _host.Context.RenderComponent<DeveloperDashboard>();

        cut.FindAll("button[role=radio]").Single(b => b.TextContent.Trim() == "kanban.viewkanban").Click();

        var titles = cut.FindAll("[data-column-title]").Select(h => h.TextContent).ToList();
        titles.Should().HaveCount(8).And.Contain("# status.inprogress");
        cut.FindAll("[data-kanban-card]").Should().HaveCount(2);
    }

    [Test]
    public void Kanban_card_avatars_do_not_repeat_the_name_for_screen_readers()
    {
        var ticket = new Ticket { Id = 5, Title = "x", Author = new User { Login = "olena" }, Developer = new User { Login = "ivan" } };

        var cut = _host.Context.RenderComponent<KanbanCard>(p => p.Add(x => x.Ticket, ticket));

        cut.FindAll("[data-avatar]").Should().HaveCount(2).And.OnlyContain(a => a.GetAttribute("aria-hidden") == "true");
    }
}
