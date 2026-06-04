using System.Linq.Expressions;

namespace ServiceDeskSystem.Domain.Interfaces
{
    public interface IReadRepository<T>
        where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
    }
}
