using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface ITechStackRepository : IRepository<TechStack>
{
    Task<IEnumerable<TechStack>> GetAllWithProductsAsync();

    Task<TechStack?> GetByIdWithProductsAsync(int id);
}
