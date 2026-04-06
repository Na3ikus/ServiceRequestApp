using System.Linq.Expressions;

namespace ServiceDeskSystem.Domain.Interfaces
{
    public interface IReadRepository<T>
        where T : class
    {
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();

        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        T? GetById(int id);
        Task<T?> GetByIdAsync(int id);
    }
}
