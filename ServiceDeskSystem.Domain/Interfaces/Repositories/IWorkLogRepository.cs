using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Domain.Interfaces.Repositories;

public interface IWorkLogRepository : IRepository<WorkLog>
{
    Task<IEnumerable<WorkLog>> GetByTicketIdAsync(int ticketId);
    Task<IEnumerable<WorkLog>> GetByUserIdAsync(int userId);
    Task<int> GetTotalTimeSpentForTicketAsync(int ticketId);
    Task<int> GetTotalTimeSpentForUserAsync(int userId);
}
