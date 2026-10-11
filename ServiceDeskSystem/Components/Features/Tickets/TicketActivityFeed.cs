using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Features.Tickets;

#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
#pragma warning disable SA1402 // The event records belong with the feed that builds them.

/// <summary>
/// One entry of the ticket activity feed; <see cref="At"/> is UTC.
/// </summary>
public abstract record TicketActivityEvent(DateTime At);

/// <summary>
/// The ticket was created (<c>@author created · type · priority</c>).
/// </summary>
public sealed record TicketCreatedActivity(DateTime At, string? AuthorLogin, TicketType Type, TicketPriority Priority)
    : TicketActivityEvent(At);

/// <summary>
/// The status changed; <see cref="ActorLogin"/> is <c>null</c> for rows backfilled from the audit log.
/// </summary>
public sealed record TicketStatusActivity(DateTime At, string? ActorLogin, TicketStatus From, TicketStatus To)
    : TicketActivityEvent(At);

/// <summary>
/// A comment (public or internal).
/// </summary>
public sealed record TicketCommentActivity(DateTime At, Comment Comment)
    : TicketActivityEvent(At);

/// <summary>
/// Merges the creation, the status history and the comments of a ticket into one timeline.
/// Assignment changes are not stored, so they are not part of it.
/// </summary>
public static class TicketActivityFeed
{
    /// <summary>
    /// Events oldest first; at equal times creation comes first, then status changes, then comments.
    /// Internal comments are left out unless <paramref name="includeInternal"/> (admin/developer).
    /// </summary>
    public static IReadOnlyList<TicketActivityEvent> Build(Ticket ticket, IEnumerable<TicketStatusHistory> history, bool includeInternal)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentNullException.ThrowIfNull(history);

        var events = new List<TicketActivityEvent>
        {
            new TicketCreatedActivity(ticket.CreatedAt, ticket.Author?.Login, ticket.Type, ticket.Priority),
        };

        events.AddRange(history
            .OrderBy(h => h.ChangedAt)
            .ThenBy(h => h.Id)
            .Select(h => new TicketStatusActivity(h.ChangedAt, h.ChangedByUser?.Login, h.OldStatus, h.NewStatus)));

        events.AddRange((ticket.Comments ?? [])
            .Where(c => includeInternal || !c.IsInternal)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Select(c => new TicketCommentActivity(c.CreatedAt, c)));

        // OrderBy is stable, so the kind order above breaks ties.
        return events.OrderBy(e => e.At).ToList();
    }
}

#pragma warning restore SA1402
#pragma warning restore SA1313
