using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface ISlaPolicyRepository : IRepository<SlaPolicy>
{
    Task<IEnumerable<SlaPolicy>> GetAllWithProductAsync();

    Task<SlaPolicy?> GetByIdWithProductAsync(int id);

    Task<IEnumerable<SlaPolicy>> GetActiveForPriorityAsync(TicketPriority priority);

    Task<bool> ExistsForScopeAsync(TicketPriority priority, int? productId, int? excludeId = null);
}
