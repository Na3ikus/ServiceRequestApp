using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Domain.Interfaces.Repositories;

public interface IWorkLogRepository : IRepository<WorkLog>
{
    Task<IEnumerable<WorkLog>> GetByTicketIdAsync(int ticketId);
    Task<IEnumerable<WorkLog>> GetByUserIdAsync(int userId);
    Task<int> GetTotalTimeSpentForTicketAsync(int ticketId);
    Task<int> GetTotalTimeSpentForUserAsync(int userId);
    Task<List<(DateTime DateLogged, int Minutes)>> GetEntriesByUserSinceAsync(int userId, DateTime since);

    /// <summary>Work log entries of all users dated at or after <paramref name="sinceUtc"/>.</summary>
    Task<List<(int UserId, DateTime DateLogged, int Minutes)>> GetEntriesSinceAsync(DateTime sinceUtc);
}
