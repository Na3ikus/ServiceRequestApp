namespace ServiceDeskSystem.Domain.Interfaces
{
    public interface IWriteRepository<in T>
        where T : class
    {
        Task CreateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
