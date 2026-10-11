namespace ServiceDeskSystem.Application.Services.Time;

/// <summary>
/// Time zone of the user's browser for one circuit. Until the browser reports it, the server zone is used.
/// </summary>
public interface IUserTimeZone
{
    event EventHandler? Changed;

    TimeZoneInfo Zone { get; }

    /// <summary>
    /// Gets the current date in the user's zone.
    /// </summary>
    DateOnly Today { get; }

    /// <summary>
    /// Switches to the given IANA (or Windows) zone id; unknown ids are ignored.
    /// </summary>
    void Set(string? zoneId);

    /// <summary>
    /// Converts a stored UTC timestamp (EF returns them as <see cref="DateTimeKind.Unspecified"/>) to the user's zone.
    /// </summary>
    DateTime ToLocal(DateTime utc);
}
