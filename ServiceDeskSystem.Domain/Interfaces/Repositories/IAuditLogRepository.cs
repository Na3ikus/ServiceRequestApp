using ServiceDeskSystem.Domain.Entities;
namespace ServiceDeskSystem.Domain.Interfaces;
public interface IAuditLogRepository : IRepository<AuditLog> {
    Task<List<AuditLog>> GetLatestLogsAsync(int count);
    Task ClearAllLogsAsync();
}
