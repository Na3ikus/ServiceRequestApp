using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.Features.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Tickets;

[TestFixture]
public class TicketFilterTests
{
    private static readonly Ticket[] Tickets =
    [
        new() { Id = 1, Title = "Login fails", Description = "", Type = TicketType.Bug, Priority = TicketPriority.Critical, Status = TicketStatus.Open, Author = new User { Login = "olena" } },
        new() { Id = 2, Title = "Export", Description = "csv broken", Type = TicketType.Support, Priority = TicketPriority.Low, Status = TicketStatus.InProgress, Product = new Product { Name = "Portal" } },
        new() { Id = 3, Title = "Old", Description = "", Type = TicketType.Bug, Priority = TicketPriority.Medium, Status = TicketStatus.Closed },
        new() { Id = 4, Title = "Done", Description = "", Type = TicketType.Project, Priority = TicketPriority.High, Status = TicketStatus.Resolved },
    ];

    private static IEnumerable<int> Ids(TicketFilterState state) => TicketFilter.Apply(Tickets, state).Select(t => t.Id);

    [Test]
    public void Empty_state_keeps_everything_and_is_inactive()
    {
        var state = new TicketFilterState();

        Ids(state).Should().Equal(1, 2, 3, 4);
        state.IsActive.Should().BeFalse();
    }

    [TestCase("login", 1)]
    [TestCase("CSV", 2)]
    [TestCase("portal", 2)]
    [TestCase("olena", 1)]
    public void Search_covers_title_description_product_and_author(string search, int expected) =>
        Ids(new TicketFilterState(Search: search)).Should().Equal(expected);

    [Test]
    public void Selects_combine()
    {
        var state = new TicketFilterState(Type: "Bug", Priority: "Critical");

        Ids(state).Should().Equal(1);
        state.IsActive.Should().BeTrue();
    }

    [TestCase("InProgress", new[] { 2 })]
    [TestCase(TicketFilter.OpenOrInProgress, new[] { 1, 2 })]
    [TestCase(TicketFilter.ClosedOrResolved, new[] { 3, 4 })]
    public void Status_options_include_the_combined_ones(string status, int[] expected) =>
        Ids(new TicketFilterState(Status: status)).Should().Equal(expected);
}
