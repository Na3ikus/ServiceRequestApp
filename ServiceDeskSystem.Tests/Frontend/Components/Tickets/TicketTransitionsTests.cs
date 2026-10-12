using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.Features.Tickets;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Tickets;

[TestFixture]
public class TicketTransitionsTests
{
    private static IEnumerable<TicketStatus> Targets(TicketStatus status) => TicketTransitions.For(status).Select(t => t.Target);

    [TestCase(TicketStatus.New)]
    [TestCase(TicketStatus.Open)]
    public void New_and_open_tickets_start_progress_or_close(TicketStatus status) =>
        Targets(status).Should().Equal(TicketStatus.InProgress, TicketStatus.Closed);

    [Test]
    public void In_progress_offers_testing_first_then_review_resolve_and_close() =>
        Targets(TicketStatus.InProgress).Should().Equal(TicketStatus.Testing, TicketStatus.CodeReview, TicketStatus.Resolved, TicketStatus.Closed);

    [Test]
    public void Testing_can_finish_go_to_review_close_or_be_rejected()
    {
        var actions = TicketTransitions.For(TicketStatus.Testing);

        actions.Select(a => a.Target).Should().Equal(TicketStatus.Done, TicketStatus.CodeReview, TicketStatus.Closed, TicketStatus.InProgress);
        actions[^1].Should().Be(new TicketTransition("statusAction.rejectTesting", TicketStatus.InProgress, TicketTransitionKind.Danger));
    }

    [Test]
    public void Code_review_approves_closes_or_rejects() =>
        TicketTransitions.For(TicketStatus.CodeReview).Select(a => a.Key).Should()
            .Equal("statusAction.approveCode", "statusAction.closeDirectly", "statusAction.rejectCode");

    [Test]
    public void Resolved_closes_or_reopens_into_progress() =>
        Targets(TicketStatus.Resolved).Should().Equal(TicketStatus.Closed, TicketStatus.InProgress);

    [Test]
    public void Done_only_closes() =>
        TicketTransitions.For(TicketStatus.Done).Should().ContainSingle()
            .Which.Should().Be(new TicketTransition("statusAction.closeTicket", TicketStatus.Closed, TicketTransitionKind.Primary));

    [Test]
    public void Closed_only_reopens_as_a_danger_action() =>
        TicketTransitions.For(TicketStatus.Closed).Should().ContainSingle()
            .Which.Should().Be(new TicketTransition("statusAction.reopenTicket", TicketStatus.Open, TicketTransitionKind.Danger));

    [Test]
    public void Every_status_has_at_most_one_primary_and_steps_back_come_last()
    {
        foreach (var status in Enum.GetValues<TicketStatus>())
        {
            var kinds = TicketTransitions.For(status).Select(a => a.Kind).ToList();

            kinds.Should().NotBeEmpty();
            kinds.Count(k => k == TicketTransitionKind.Primary).Should().BeLessThanOrEqualTo(1);
            kinds.Should().BeInAscendingOrder(because: "{0}: primary, then normal, then danger", status);
        }
    }
}
