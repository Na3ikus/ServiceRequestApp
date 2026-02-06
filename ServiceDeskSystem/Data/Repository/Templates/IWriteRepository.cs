using System.Linq.Expressions;

namespace ServiceDeskSystem.Data.Repository.Templates
{
    public interface IWriteRepository<in T>
        where T : class
    {
        void Create(T entity);
        Task CreateAsync(T entity);

        void Update(T entity);
        Task UpdateAsync(T entity);

        void Delete(int id);
        Task DeleteAsync(int id);
    }
}
