using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Features.Tickets;

/// <summary>
/// Columns the ticket table can sort by.
/// </summary>
public enum TicketSort
{
    Id,
    Title,
    Type,
    Status,
    Priority,
    Author,
    Assignee,
    Created,
}

/// <summary>
/// Sortable ticket table with configurable columns; shows log rows on narrow screens.
/// </summary>
public partial class TicketTable
{
    private TicketSort sort = TicketSort.Created;
    private bool descending = true;

    [Parameter]
    public IEnumerable<Ticket> Tickets { get; set; } = [];

    [Parameter]
    public EventCallback<int> OnTicketClick { get; set; }

    [Parameter]
    public bool ShowAuthor { get; set; } = true;

    [Parameter]
    public bool ShowAssignee { get; set; } = true;

    /// <summary>
    /// Orders tickets by a column; ties fall back to the newest first.
    /// </summary>
    public static IReadOnlyList<Ticket> Order(IEnumerable<Ticket> tickets, TicketSort sort, bool descending)
    {
        var comparer = StringComparer.CurrentCultureIgnoreCase;
        var ordered = sort switch
        {
            TicketSort.Id => Direction(tickets, t => t.Id, descending),
            TicketSort.Title => Direction(tickets, t => t.Title, descending, comparer),
            TicketSort.Type => Direction(tickets, t => t.Type, descending),
            TicketSort.Status => Direction(tickets, t => t.Status, descending),
            TicketSort.Priority => Direction(tickets, t => t.Priority, descending),
            TicketSort.Author => Direction(tickets, t => t.Author?.Login ?? string.Empty, descending, comparer),

            // Unassigned tickets stay at the bottom in both directions.
            TicketSort.Assignee => tickets.OrderBy(t => t.Developer is null)
                .ThenBy(t => t.Developer?.Login ?? string.Empty, descending ? Reverse(comparer) : comparer),
            _ => Direction(tickets, t => t.CreatedAt, descending),
        };

        return ordered.ThenByDescending(t => t.CreatedAt).ToList();
    }

    private static IOrderedEnumerable<Ticket> Direction<TKey>(IEnumerable<Ticket> tickets, Func<Ticket, TKey> key, bool descending, IComparer<TKey>? comparer = null) =>
        descending ? tickets.OrderByDescending(key, comparer) : tickets.OrderBy(key, comparer);

    private static Comparer<string> Reverse(StringComparer comparer) =>
        Comparer<string>.Create((a, b) => comparer.Compare(b, a));

    private IReadOnlyList<Ticket> Sorted() => Order(this.Tickets, this.sort, this.descending);

    private void SortBy(TicketSort column)
    {
        if (this.sort == column)
        {
            this.descending = !this.descending;
            return;
        }

        this.sort = column;

        // Dates, ids and priority read best newest/highest first; text columns start A→Z.
        this.descending = column is TicketSort.Created or TicketSort.Id or TicketSort.Priority;
    }
}
