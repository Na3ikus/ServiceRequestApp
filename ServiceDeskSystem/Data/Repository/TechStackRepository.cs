using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Data.Repository.Templates;

namespace ServiceDeskSystem.Data.Repository
{
    internal sealed class TechStackRepository : TemplateRepository<TechStack>
    {
        public TechStackRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<TechStack> DbSet => this.Context.TechStacks;

        public IEnumerable<TechStack> GetAllWithProducts()
        {
            return this.Context.TechStacks
                .Include(t => t.Products)
                .OrderBy(t => t.Name)
                .ToList();
        }

        public async Task<IEnumerable<TechStack>> GetAllWithProductsAsync()
        {
            return await this.Context.TechStacks
                .Include(t => t.Products)
                .OrderBy(t => t.Name)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<TechStack?> GetByIdWithProductsAsync(int id)
        {
            return await this.Context.TechStacks
                .Include(t => t.Products)
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);
        }
    }
}
