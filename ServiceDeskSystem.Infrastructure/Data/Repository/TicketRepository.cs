using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public sealed class TicketRepository : TemplateRepository<Ticket>
    {
        public TicketRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<Ticket> DbSet => this.Context.Tickets;

        public IEnumerable<Ticket> GetAllWithIncludes()
        {
            return this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Product)
                .Include(t => t.Developer)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public async Task<IEnumerable<Ticket>> GetAllWithIncludesAsync()
        {
            return await this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Product)
                .Include(t => t.Developer)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public Ticket? GetByIdWithIncludes(int id)
        {
            return this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Product)
                .Include(t => t.Developer)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefault(t => t.Id == id);
        }

        public async Task<Ticket?> GetByIdWithIncludesAsync(int id)
        {
            return await this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Product)
                .Include(t => t.Developer)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Ticket>> GetByDeveloperIdAsync(int developerId)
        {
            return await this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Product)
                .Where(t => t.DeveloperId == developerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}

