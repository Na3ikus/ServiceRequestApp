using System.Linq.Expressions;

namespace ServiceDeskSystem.Data.Repository.Templates
{
    public interface IReadRepository<T>
        where T : class
    {
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();

        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        T? GetById(long id);
        Task<T?> GetByIdAsync(long id);
    }
}
