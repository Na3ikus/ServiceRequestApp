using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
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

    public async Task<List<TicketStatusHistory>> GetByTicketAsync(int ticketId)
    {
        return await this.DbSet
            .AsNoTracking()
            .Include(h => h.ChangedByUser)
            .Where(h => h.TicketId == ticketId)
            .OrderBy(h => h.ChangedAt)
            .ThenBy(h => h.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Dictionary<int, DateTime>> GetFirstResolvedTimesAsync()
    {
        var rows = await this.DbSet
            .Where(h => h.NewStatus == TicketStatus.Resolved || h.NewStatus == TicketStatus.Closed || h.NewStatus == TicketStatus.Done)
            .GroupBy(h => h.TicketId)
            .Select(g => new { TicketId = g.Key, ChangedAt = g.Min(h => h.ChangedAt) })
            .ToListAsync()
            .ConfigureAwait(false);

        return rows.ToDictionary(r => r.TicketId, r => r.ChangedAt);
    }
}
