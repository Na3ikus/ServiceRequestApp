using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Calendar;

/// <summary>
/// What a calendar entry marks: the start (or contact) date, a deadline, or a deadline already missed.
/// </summary>
public enum CalendarEventKind
{
    Start,
    Due,
    Overdue,
}

#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
public sealed record CalendarEvent(Ticket Ticket, DateOnly Date, CalendarEventKind Kind);
#pragma warning restore SA1313

/// <summary>
/// Places tickets on calendar days: the start date (or creation) and the due date, both stored as UTC
/// and shown on the user's local day.
/// </summary>
public static class CalendarEvents
{
    /// <summary>
    /// Events of one month grouped by local date; deadlines first, then by ticket id.
    /// </summary>
    public static IReadOnlyDictionary<DateOnly, IReadOnlyList<CalendarEvent>> ForMonth(
        IEnumerable<Ticket> tickets, int year, int month, Func<DateTime, DateTime> toLocal, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(tickets);
        ArgumentNullException.ThrowIfNull(toLocal);

        var events = new List<CalendarEvent>();
        foreach (var ticket in tickets)
        {
            var start = DateOnly.FromDateTime(toLocal(ticket.StartDate ?? ticket.CreatedAt));
            events.Add(new CalendarEvent(ticket, start, CalendarEventKind.Start));

            if (ticket.DueDate is { } due)
            {
                var kind = due < nowUtc && !IsFinished(ticket.Status) ? CalendarEventKind.Overdue : CalendarEventKind.Due;
                var dueDay = DateOnly.FromDateTime(toLocal(due));
                if (dueDay == start)
                {
                    // One entry per ticket and day: the deadline says more than the start.
                    events.RemoveAt(events.Count - 1);
                }

                events.Add(new CalendarEvent(ticket, dueDay, kind));
            }
        }

        return events
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .GroupBy(e => e.Date)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<CalendarEvent>)g.OrderBy(e => e.Kind == CalendarEventKind.Start).ThenBy(e => e.Ticket.Id).ToList());
    }

    private static bool IsFinished(TicketStatus status) =>
        status is TicketStatus.Resolved or TicketStatus.Done or TicketStatus.Closed;
}
