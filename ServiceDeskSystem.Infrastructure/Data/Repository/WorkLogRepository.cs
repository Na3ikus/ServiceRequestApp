using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces.Repositories;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;

namespace ServiceDeskSystem.Infrastructure.Data.Repository;

public sealed class WorkLogRepository : TemplateRepository<WorkLog>, IWorkLogRepository
{
    public WorkLogRepository(BugTrackerDbContext context)
        : base(context)
    {
    }

    protected override DbSet<WorkLog> DbSet => this.Context.WorkLogs;

    public async Task<IEnumerable<WorkLog>> GetByTicketIdAsync(int ticketId)
    {
        return await this.DbSet
            .Include(w => w.User)
            .ThenInclude(u => u.Person)
            .Where(w => w.TicketId == ticketId)
            .OrderByDescending(w => w.DateLogged)
            .ThenByDescending(w => w.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<WorkLog>> GetByUserIdAsync(int userId)
    {
        return await this.DbSet
            .Include(w => w.Ticket)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.DateLogged)
            .ThenByDescending(w => w.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<int> GetTotalTimeSpentForTicketAsync(int ticketId)
    {
        return await this.DbSet
            .Where(w => w.TicketId == ticketId)
            .SumAsync(w => w.TimeSpentMinutes)
            .ConfigureAwait(false);
    }

    public async Task<int> GetTotalTimeSpentForUserAsync(int userId)
    {
        return await this.DbSet
            .Where(w => w.UserId == userId)
            .SumAsync(w => w.TimeSpentMinutes)
            .ConfigureAwait(false);
    }
}
