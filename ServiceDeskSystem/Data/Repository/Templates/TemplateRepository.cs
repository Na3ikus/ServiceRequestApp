using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ServiceDeskSystem.Data.Repository.Templates
{
    internal abstract class TemplateRepository<T> : IReadRepository<T>, IWriteRepository<T>
        where T : class
    {
        protected readonly BugTrackerDbContext Context;

        protected TemplateRepository(BugTrackerDbContext context)
        {
            this.Context = context;
        }

        protected abstract DbSet<T> DbSet { get; }

        public virtual IEnumerable<T> GetAll()
        {
            return this.DbSet.ToList();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await this.DbSet.ToListAsync().ConfigureAwait(false);
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return this.DbSet.Where(predicate).ToList();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await this.DbSet.Where(predicate).ToListAsync().ConfigureAwait(false);
        }

        public virtual T? GetById(long id)
        {
            return this.DbSet.Find(id);
        }

        public virtual async Task<T?> GetByIdAsync(long id)
        {
            return await this.DbSet.FindAsync(id).ConfigureAwait(false);
        }

        public virtual void Create(T entity)
        {
            this.DbSet.Add(entity);
        }

        public virtual async Task CreateAsync(T entity)
        {
            await this.DbSet.AddAsync(entity).ConfigureAwait(false);
        }

        public virtual void Update(T entity)
        {
            this.DbSet.Update(entity);
        }

        public virtual Task UpdateAsync(T entity)
        {
            this.DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual void Delete(long id)
        {
            var entity = this.DbSet.Find(id);
            if (entity != null)
            {
                this.DbSet.Remove(entity);
            }
        }

        public virtual async Task DeleteAsync(long id)
        {
            var entity = await this.DbSet.FindAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                this.DbSet.Remove(entity);
            }
        }
    }
}
