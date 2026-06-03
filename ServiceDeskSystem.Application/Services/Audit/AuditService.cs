using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.Services.Audit;

public sealed class AuditService(IRepositoryFacadeFactory repositoryFacadeFactory) : IAuditService
{
    public async Task LogActionAsync(string action, string entityName, string entityId, string? changes = null, int? userId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        
        var log = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Changes = changes,
            Timestamp = DateTime.UtcNow,
            UserId = userId
        };

        await repo.AuditLogs.CreateAsync(log).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<List<AuditLog>> GetLatestLogsAsync(int count = 100)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.AuditLogs.GetLatestLogsAsync(count).ConfigureAwait(false);
    }

    public async Task ClearAllLogsAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        await repo.AuditLogs.ClearAllLogsAsync().ConfigureAwait(false);
    }
}

