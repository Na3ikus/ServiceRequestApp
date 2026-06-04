using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Infrastructure.Data.Repository.Templates
{
    public abstract class TemplateRepository<T> : IReadRepository<T>, IWriteRepository<T>
        where T : class
    {
        protected readonly BugTrackerDbContext Context;

        protected TemplateRepository(BugTrackerDbContext context)
        {
            this.Context = context;
        }

        protected abstract DbSet<T> DbSet { get; }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await this.DbSet.ToListAsync().ConfigureAwait(false);
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await this.DbSet.FindAsync(id).ConfigureAwait(false);
        }

        public virtual async Task CreateAsync(T entity)
        {
            await this.DbSet.AddAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await this.DbSet.FindAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                this.DbSet.Remove(entity);
            }
        }
    }
}
