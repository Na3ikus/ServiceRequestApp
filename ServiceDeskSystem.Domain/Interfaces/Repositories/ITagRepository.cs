using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<IEnumerable<Tag>> GetAllWithTicketsAsync();

    Task<Tag?> GetByIdWithTicketsAsync(int id);

    Task<Tag?> GetByNameAsync(string name);
}
