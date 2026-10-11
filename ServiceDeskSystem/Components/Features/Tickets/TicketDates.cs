using ServiceDeskSystem.Application.Services.Time;

namespace ServiceDeskSystem.Components.Features.Tickets;

/// <summary>
/// Start and due dates are stored as UTC (SLA deadlines are computed in UTC); the
/// <c>datetime-local</c> inputs show and accept the user's wall-clock time.
/// </summary>
public static class TicketDates
{
    /// <summary>
    /// Stored UTC value → input value in the user's zone, trimmed to minutes like the input.
    /// </summary>
    public static DateTime? ToInput(DateTime? utc, IUserTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        if (utc is not { } value)
        {
            return null;
        }

        var local = zone.ToLocal(value);
        return new DateTime(local.Year, local.Month, local.Day, local.Hour, local.Minute, 0, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Input value in the user's zone → UTC for storage.
    /// </summary>
    public static DateTime? ToStorage(DateTime? local, IUserTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        return local is { } value ? zone.ToUtc(value) : null;
    }
}
