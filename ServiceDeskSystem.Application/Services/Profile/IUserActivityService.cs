using ServiceDeskSystem.Application.Services.Profile.Models;

namespace ServiceDeskSystem.Application.Services.Profile;

public interface IUserActivityService
{
    /// <summary>
    /// Per-day activity of a user between <paramref name="from"/> and <paramref name="to"/> (inclusive,
    /// calendar dates in <paramref name="zone"/>, the user's time zone). Only days with at least one action
    /// are returned, ordered by date.
    /// </summary>
    Task<IReadOnlyList<DailyActivityDto>> GetDailyActivityAsync(int userId, DateOnly from, DateOnly to, TimeZoneInfo zone);
}
