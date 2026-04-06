using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetAllWithIncludesAsync();

    Task<Ticket?> GetByIdWithIncludesAsync(int id);

    Task<IEnumerable<Ticket>> GetByDeveloperIdAsync(int developerId);
}
