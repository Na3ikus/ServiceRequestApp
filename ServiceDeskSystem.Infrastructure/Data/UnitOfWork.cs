using ServiceDeskSystem.Domain.Interfaces;
namespace ServiceDeskSystem.Infrastructure.Data
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

        public void Dispose()
        {
            this._context.Dispose();
        }
    }
}

