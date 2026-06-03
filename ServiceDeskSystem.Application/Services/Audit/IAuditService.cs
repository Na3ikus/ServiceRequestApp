using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Audit;

public interface IAuditService
{
    Task LogActionAsync(string action, string entityName, string entityId, string? changes = null, int? userId = null);
    Task<List<AuditLog>> GetLatestLogsAsync(int count = 100);
    Task ClearAllLogsAsync();
}

public static class AuditServiceExtensions
{
    public static Task LogActionSafeAsync(
        this IAuditService? auditService,
        string action,
        string entityName,
        string entityId,
        string? changes = null,
        int? userId = null)
    {
        return auditService?.LogActionAsync(action, entityName, entityId, changes, userId) ?? Task.CompletedTask;
    }
}

