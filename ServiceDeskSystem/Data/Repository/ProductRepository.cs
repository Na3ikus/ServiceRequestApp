using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Data.Repository.Templates;

namespace ServiceDeskSystem.Data.Repository
{
    internal sealed class ProductRepository : TemplateRepository<Product>
    {
        public ProductRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<Product> DbSet => this.Context.Products;

        public IEnumerable<Product> GetAllWithTechStack()
        {
            return this.Context.Products
                .Include(p => p.TechStack)
                .OrderBy(p => p.Name)
                .ToList();
        }

        public async Task<IEnumerable<Product>> GetAllWithTechStackAsync()
        {
            return await this.Context.Products
                .Include(p => p.TechStack)
                .OrderBy(p => p.Name)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Product?> GetByIdWithTicketsAsync(long id)
        {
            return await this.Context.Products
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.Id == id)
                .ConfigureAwait(false);
        }
    }
}
