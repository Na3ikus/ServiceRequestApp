using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;

namespace ServiceDeskSystem.Infrastructure.Data.Repository;

public sealed class TicketStatusHistoryRepository : TemplateRepository<TicketStatusHistory>, ITicketStatusHistoryRepository
{
    public TicketStatusHistoryRepository(BugTrackerDbContext context)
        : base(context)
    {
    }

    protected override DbSet<TicketStatusHistory> DbSet => this.Context.TicketStatusHistory;

    public async Task<List<DateTime>> GetChangeTimesByUserAsync(int userId, DateTime sinceUtc)
    {
        return await this.DbSet
            .Where(h => h.ChangedByUserId == userId && h.ChangedAt >= sinceUtc)
            .Select(h => h.ChangedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
