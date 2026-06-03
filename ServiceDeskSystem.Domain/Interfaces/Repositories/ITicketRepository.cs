using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetAllWithIncludesAsync();
    Task<(IEnumerable<Ticket> Items, int TotalCount)> GetPagedWithIncludesAsync(int page, int pageSize);

    Task<Ticket?> GetByIdWithIncludesAsync(int id);

    Task<IEnumerable<Ticket>> GetByDeveloperIdAsync(int developerId);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByStatusAsync(TicketStatus status);
    Task<int> GetCountByPriorityAsync(TicketPriority priority);
    Task<int> GetCountByAuthorIdAsync(int authorId);
    Task<int> GetCountByDeveloperIdAsync(int developerId);
    Task<int> GetDeveloperInProgressCountAsync(int developerId);
    Task<int> GetDeveloperCompletedCountAsync(int developerId);
    Task<Dictionary<TicketStatus, int>> GetTicketCountGroupedByStatusAsync();
    Task<Dictionary<TicketPriority, int>> GetTicketCountGroupedByPriorityAsync();
}


