using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.Features.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Tickets;

[TestFixture]
public class TicketTableTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    private static Ticket Ticket(int id, string title, TicketPriority priority, int day, string? developer = null, bool assessed = true) => new()
    {
        Id = id,
        Title = title,
        Priority = priority,
        CreatedAt = new DateTime(2026, 10, day, 9, 0, 0, DateTimeKind.Utc),
        Author = new User { Login = "author" + id },
        Developer = developer is null ? null : new User { Login = developer },
        IsPriorityAssessed = assessed,
    };

    private static readonly Ticket[] Tickets =
    [
        Ticket(1, "Bravo", TicketPriority.Low, 3, "zed"),
        Ticket(2, "alpha", TicketPriority.Critical, 1, null, assessed: false),
        Ticket(3, "Charlie", TicketPriority.Medium, 5, "amy"),
    ];

    private IRenderedComponent<TicketTable> Render(bool showAuthor = true, bool showAssignee = true) =>
        _host.Context.RenderComponent<TicketTable>(p => p
            .Add(x => x.Tickets, Tickets)
            .Add(x => x.ShowAuthor, showAuthor)
            .Add(x => x.ShowAssignee, showAssignee));

    private static List<string> RowIds(IRenderedComponent<TicketTable> cut) =>
        cut.FindAll("tr[data-ticket-row] td:first-child").Select(td => td.TextContent.Trim()).ToList();

    [Test]
    public void Defaults_to_newest_first_with_aria_sort()
    {
        var cut = Render();

        RowIds(cut).Should().Equal("#3", "#1", "#2");
        cut.Find("button[data-sort=Created]").ParentElement!.GetAttribute("aria-sort").Should().Be("descending");
        cut.Find("button[data-sort=Title]").ParentElement!.GetAttribute("aria-sort").Should().Be("none");
    }

    [Test]
    public void Clicking_a_header_sorts_and_clicking_again_flips()
    {
        var cut = Render();

        cut.Find("button[data-sort=Title]").Click();
        RowIds(cut).Should().Equal("#2", "#1", "#3");
        cut.Find("button[data-sort=Title]").ParentElement!.GetAttribute("aria-sort").Should().Be("ascending");

        cut.Find("button[data-sort=Title]").Click();
        RowIds(cut).Should().Equal("#3", "#1", "#2");
    }

    [Test]
    public void Priority_starts_highest_first()
    {
        var cut = Render();

        cut.Find("button[data-sort=Priority]").Click();

        RowIds(cut).Should().Equal("#2", "#3", "#1");
    }

    [Test]
    public void Unassigned_stay_last_in_both_directions()
    {
        TicketTable.Order(Tickets, TicketSort.Assignee, false).Select(t => t.Id).Should().Equal(3, 1, 2);
        TicketTable.Order(Tickets, TicketSort.Assignee, true).Select(t => t.Id).Should().Equal(1, 3, 2);
    }

    [Test]
    public void Columns_follow_the_flags()
    {
        Render().FindAll("th").Should().HaveCount(8);
        Render(showAuthor: false).FindAll("button[data-sort=Author]").Should().BeEmpty();
        Render(showAssignee: false).FindAll("button[data-sort=Assignee]").Should().BeEmpty();
    }

    [Test]
    public void Titles_are_links_and_staff_see_the_triage_mark()
    {
        var cut = Render();

        cut.FindAll("tr[data-ticket-row] a").Select(a => a.GetAttribute("href")).Should().Contain("/ticket/2");
        cut.FindAll("tr[data-ticket-row] [data-triage]").Should().ContainSingle()
            .Which.TextContent.Should().Be("[tickets.triagerequired]");
    }
}
