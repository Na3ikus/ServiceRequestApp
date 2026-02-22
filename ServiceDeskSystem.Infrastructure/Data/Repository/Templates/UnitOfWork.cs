using ServiceDeskSystem.Domain.Interfaces;
namespace ServiceDeskSystem.Infrastructure.Data.Repository.Templates
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly BugTrackerDbContext _context;

        public UnitOfWork(BugTrackerDbContext context)
        {
            this._context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await this._context.SaveChangesAsync().ConfigureAwait(false);
        }

        public int SaveChanges()
        {
            return this._context.SaveChanges();
        }

        public void Dispose()
        {
            this._context.Dispose();
        }
    }
}

