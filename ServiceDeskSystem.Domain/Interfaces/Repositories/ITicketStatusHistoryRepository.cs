using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface ITicketStatusHistoryRepository : IRepository<TicketStatusHistory>
{
    /// <summary>Times (UTC) of status changes made by the user at or after <paramref name="sinceUtc"/>.</summary>
    Task<List<DateTime>> GetChangeTimesByUserAsync(int userId, DateTime sinceUtc);

    /// <summary>Status changes of one ticket, oldest first, with the user who made each change.</summary>
    Task<List<TicketStatusHistory>> GetByTicketAsync(int ticketId);

    /// <summary>Per ticket id, the time (UTC) of its first change to <c>Resolved</c>, <c>Closed</c> or <c>Done</c>.</summary>
    Task<Dictionary<int, DateTime>> GetFirstResolvedTimesAsync();
}
