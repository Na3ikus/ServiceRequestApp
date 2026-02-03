namespace ServiceDeskSystem.Data.Repository.Templates
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}
