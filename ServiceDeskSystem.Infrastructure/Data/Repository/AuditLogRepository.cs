using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public class AuditLogRepository : TemplateRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(BugTrackerDbContext context) : base(context) {}

        protected override DbSet<AuditLog> DbSet => this.Context.AuditLogs;

        public async Task<List<AuditLog>> GetLatestLogsAsync(int count) {
            return await this.Context.AuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        public async Task ClearAllLogsAsync() {
            try {
                await this.Context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE AuditLogs").ConfigureAwait(false);
            } catch {
                this.Context.AuditLogs.RemoveRange(this.Context.AuditLogs);
                await this.Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
