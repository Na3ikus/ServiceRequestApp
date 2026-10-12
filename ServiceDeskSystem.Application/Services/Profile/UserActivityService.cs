using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.Services.Profile;

/// <summary>
/// Builds the profile activity map from four permanent sources: tickets created, comments,
/// status changes (<c>TicketStatusHistory</c>, not the clearable audit log) and work logs.
/// </summary>
public sealed class UserActivityService(IRepositoryFacadeFactory repositoryFacadeFactory) : IUserActivityService
{
    public async Task<IReadOnlyList<DailyActivityDto>> GetDailyActivityAsync(int userId, DateOnly from, DateOnly to, TimeZoneInfo zone)
    {
        if (to < from)
        {
            return [];
        }

        // One extra day on the UTC side covers any zone offset; exact filtering is by the date in the zone below.
        var sinceUtc = from.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetCreatedTimesByAuthorAsync(userId, sinceUtc).ConfigureAwait(false);
        var comments = await repo.Comments.GetCreatedTimesByAuthorAsync(userId, sinceUtc).ConfigureAwait(false);
        var statusChanges = await repo.TicketStatusHistory.GetChangeTimesByUserAsync(userId, sinceUtc).ConfigureAwait(false);
        var workLogs = await repo.WorkLogs.GetEntriesByUserSinceAsync(userId, sinceUtc).ConfigureAwait(false);

        var days = new SortedDictionary<DateOnly, int[]>();

        void Add(DateOnly date, int kind, int minutes = 0)
        {
            if (date < from || date > to)
            {
                return;
            }

            if (!days.TryGetValue(date, out var counts))
            {
                counts = new int[5];
                days[date] = counts;
            }

            counts[kind]++;
            counts[4] += minutes;
        }

        foreach (var t in tickets)
        {
            Add(LocalDate(t, zone), 0);
        }

        foreach (var c in comments)
        {
            Add(LocalDate(c, zone), 1);
        }

        foreach (var s in statusChanges)
        {
            Add(LocalDate(s, zone), 2);
        }

        foreach (var (dateLogged, minutes) in workLogs)
        {
            // WorkLogService stores the picked local date converted to UTC.
            Add(LocalDate(dateLogged, zone), 3, minutes);
        }

        return days
            .Select(d => new DailyActivityDto(d.Key, d.Value[0], d.Value[1], d.Value[2], d.Value[3], d.Value[4]))
            .ToList();
    }

    private static DateOnly LocalDate(DateTime utc, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone));
}
