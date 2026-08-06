using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public sealed class TagRepository : TemplateRepository<Tag>, ITagRepository
    {
        public TagRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<Tag> DbSet => this.Context.Tags;

        public async Task<IEnumerable<Tag>> GetAllWithTicketsAsync()
        {
            return await this.Context.Tags
                .Include(t => t.Tickets)
                .OrderBy(t => t.Name)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Tag?> GetByIdWithTicketsAsync(int id)
        {
            return await this.Context.Tags
                .Include(t => t.Tickets)
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await this.Context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower())
                .ConfigureAwait(false);
        }
    }
}
