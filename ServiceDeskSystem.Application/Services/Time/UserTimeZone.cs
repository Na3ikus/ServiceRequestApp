namespace ServiceDeskSystem.Application.Services.Time;

public sealed class UserTimeZone : IUserTimeZone
{
    public event EventHandler? Changed;

    public TimeZoneInfo Zone { get; private set; } = TimeZoneInfo.Local;

    public DateOnly Today => DateOnly.FromDateTime(this.ToLocal(DateTime.UtcNow));

    public void Set(string? zoneId)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
        {
            return;
        }

        TimeZoneInfo zone;
        try
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return;
        }

        if (zone.Id == this.Zone.Id)
        {
            return;
        }

        this.Zone = zone;
        this.Changed?.Invoke(this, EventArgs.Empty);
    }

    public DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), this.Zone);
}
