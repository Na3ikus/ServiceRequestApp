using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public sealed class TicketRepository : TemplateRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<Ticket> DbSet => this.Context.Tickets;

        public async Task<IEnumerable<Ticket>> GetByDeveloperIdAsync(int developerId)
        {
            return await this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Developer)
                .Include(t => t.Product)
                .Where(t => t.DeveloperId == developerId)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Ticket>> GetAllWithIncludesAsync()
        {
            return await this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Developer)
                .Include(t => t.Product)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Ticket?> GetByIdWithIncludesAsync(int id)
        {
            return await this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Developer)
                .Include(t => t.Product)
                .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<(IEnumerable<Ticket> Items, int TotalCount)> GetPagedWithIncludesAsync(int page, int pageSize)
        {
            var totalCount = await this.Context.Tickets.CountAsync().ConfigureAwait(false);
            var items = await this.Context.Tickets
                .Include(t => t.Author)
                .Include(t => t.Developer)
                .Include(t => t.Product)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
            return (items, totalCount);
        }

        public async Task<int> GetTotalCountAsync() => await this.Context.Tickets.CountAsync().ConfigureAwait(false);
        public async Task<int> GetCountByStatusAsync(TicketStatus status) => await this.Context.Tickets.CountAsync(t => t.Status == status).ConfigureAwait(false);
        public async Task<int> GetCountByPriorityAsync(TicketPriority priority) => await this.Context.Tickets.CountAsync(t => t.Priority == priority).ConfigureAwait(false);
        public async Task<int> GetCountByAuthorIdAsync(int authorId) => await this.Context.Tickets.CountAsync(t => t.AuthorId == authorId).ConfigureAwait(false);
        public async Task<int> GetCountByDeveloperIdAsync(int developerId) => await this.Context.Tickets.CountAsync(t => t.DeveloperId == developerId).ConfigureAwait(false);
        public async Task<int> GetDeveloperInProgressCountAsync(int developerId) => await this.Context.Tickets.CountAsync(t => t.DeveloperId == developerId && t.Status == TicketStatus.InProgress).ConfigureAwait(false);
        public async Task<int> GetDeveloperCompletedCountAsync(int developerId) => await this.Context.Tickets.CountAsync(t => t.DeveloperId == developerId && (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)).ConfigureAwait(false);
        public async Task<Dictionary<TicketStatus, int>> GetTicketCountGroupedByStatusAsync() {
            var counts = await this.Context.Tickets.GroupBy(t => t.Status).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count).ConfigureAwait(false);
            return counts;
        }
        public async Task<Dictionary<TicketPriority, int>> GetTicketCountGroupedByPriorityAsync() {
            var counts = await this.Context.Tickets.GroupBy(t => t.Priority).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count).ConfigureAwait(false);
            return counts;
        }
    }
}
