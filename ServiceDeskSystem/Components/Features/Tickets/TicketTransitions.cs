using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Features.Tickets;

/// <summary>
/// How a status action is shown in the ticket header: the main forward step, another
/// forward step, or a step back (reject / reopen).
/// </summary>
public enum TicketTransitionKind
{
    Primary,
    Normal,
    Danger,
}

/// <summary>
/// One status action the assignee can take; <see cref="Key"/> is the localization key of its label.
/// </summary>
#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
public sealed record TicketTransition(string Key, TicketStatus Target, TicketTransitionKind Kind);
#pragma warning restore SA1313

/// <summary>
/// The status workflow of a ticket: which actions are offered for each status.
/// </summary>
public static class TicketTransitions
{
    private static readonly TicketTransition CloseDirectly = new ("statusAction.closeDirectly", TicketStatus.Closed, TicketTransitionKind.Normal);

    /// <summary>
    /// Actions for a ticket in <paramref name="status"/>: the primary one first, then the
    /// other forward steps, then the steps back.
    /// </summary>
    public static IReadOnlyList<TicketTransition> For(TicketStatus status) => status switch
    {
        TicketStatus.New or TicketStatus.Open =>
        [
            new ("statusAction.startProgress", TicketStatus.InProgress, TicketTransitionKind.Primary),
            CloseDirectly,
        ],
        TicketStatus.InProgress =>
        [
            new ("statusAction.submitTesting", TicketStatus.Testing, TicketTransitionKind.Primary),
            new ("statusAction.submitCodeReview", TicketStatus.CodeReview, TicketTransitionKind.Normal),
            new ("statusAction.markResolved", TicketStatus.Resolved, TicketTransitionKind.Normal),
            CloseDirectly,
        ],
        TicketStatus.Testing =>
        [
            new ("statusAction.markDone", TicketStatus.Done, TicketTransitionKind.Primary),
            new ("statusAction.submitCodeReview", TicketStatus.CodeReview, TicketTransitionKind.Normal),
            CloseDirectly,
            new ("statusAction.rejectTesting", TicketStatus.InProgress, TicketTransitionKind.Danger),
        ],
        TicketStatus.CodeReview =>
        [
            new ("statusAction.approveCode", TicketStatus.Done, TicketTransitionKind.Primary),
            CloseDirectly,
            new ("statusAction.rejectCode", TicketStatus.InProgress, TicketTransitionKind.Danger),
        ],
        TicketStatus.Resolved =>
        [
            new ("statusAction.closeTicket", TicketStatus.Closed, TicketTransitionKind.Primary),
            new ("statusAction.reopenTicket", TicketStatus.InProgress, TicketTransitionKind.Danger),
        ],
        TicketStatus.Done =>
        [
            new ("statusAction.closeTicket", TicketStatus.Closed, TicketTransitionKind.Primary),
        ],
        TicketStatus.Closed =>
        [
            new ("statusAction.reopenTicket", TicketStatus.Open, TicketTransitionKind.Danger),
        ],
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };
}
