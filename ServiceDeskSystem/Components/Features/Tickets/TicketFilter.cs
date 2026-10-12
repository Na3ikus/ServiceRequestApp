using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Features.Tickets;

/// <summary>
/// What the ticket list filter bar asks for. "All" means no restriction for the selects.
/// </summary>
#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
public sealed record TicketFilterState(string Search = "", string Type = TicketFilter.All, string Priority = TicketFilter.All, string Status = TicketFilter.All)
#pragma warning restore SA1313
{
    public bool IsActive =>
        !string.IsNullOrWhiteSpace(this.Search) || this.Type != TicketFilter.All || this.Priority != TicketFilter.All || this.Status != TicketFilter.All;
}

/// <summary>
/// In-memory ticket filtering shared by the ticket list pages.
/// </summary>
public static class TicketFilter
{
    public const string All = "All";

    /// <summary>Status option matching Open and In Progress.</summary>
    public const string OpenOrInProgress = "Open/InProgress";

    /// <summary>Status option matching Closed and Resolved.</summary>
    public const string ClosedOrResolved = "Closed/Resolved";

    public static IReadOnlyList<Ticket> Apply(IEnumerable<Ticket> tickets, TicketFilterState state)
    {
        ArgumentNullException.ThrowIfNull(tickets);
        ArgumentNullException.ThrowIfNull(state);

        var query = tickets;
        var search = state.Search.Trim();

        if (search.Length > 0)
        {
            query = query.Where(t =>
                t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Product?.Name.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                t.Author?.Login.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
        }

        if (Enum.TryParse<TicketType>(state.Type, out var type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (Enum.TryParse<TicketPriority>(state.Priority, out var priority))
        {
            query = query.Where(t => t.Priority == priority);
        }

        query = state.Status switch
        {
            All => query,
            OpenOrInProgress => query.Where(t => t.Status is TicketStatus.Open or TicketStatus.InProgress),
            ClosedOrResolved => query.Where(t => t.Status is TicketStatus.Closed or TicketStatus.Resolved),
            _ when Enum.TryParse<TicketStatus>(state.Status, out var status) => query.Where(t => t.Status == status),
            _ => query,
        };

        return query.ToList();
    }
}
