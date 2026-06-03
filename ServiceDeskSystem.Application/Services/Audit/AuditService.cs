using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Application.Services.Audit;

public sealed class AuditService(IDbContextFactory<BugTrackerDbContext> contextFactory) : IAuditService
{
    public async Task LogActionAsync(string action, string entityName, string entityId, string? changes = null, int? userId = null)
    {
        await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        
        var log = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Changes = changes,
            Timestamp = DateTime.UtcNow,
            UserId = userId
        };

        context.AuditLogs.Add(log);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<List<AuditLog>> GetLatestLogsAsync(int count = 100)
    {
        await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        
        return await context.AuditLogs
            .AsNoTracking()
            .Include(l => l.User)
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task ClearAllLogsAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        try
        {
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE AuditLogs").ConfigureAwait(false);
        }
        catch
        {
            context.AuditLogs.RemoveRange(context.AuditLogs);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}

