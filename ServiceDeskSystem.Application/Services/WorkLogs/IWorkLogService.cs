using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.WorkLogs;

public interface IWorkLogService
{
    Task<(bool Success, string? ErrorMessage)> AddWorkLogAsync(int ticketId, int userId, int timeSpentMinutes, DateTime dateLogged, string description);
    Task<IEnumerable<WorkLog>> GetWorkLogsByTicketIdAsync(int ticketId);
    Task<IEnumerable<WorkLog>> GetWorkLogsByUserIdAsync(int userId);
    Task<int> GetTotalTimeSpentForTicketAsync(int ticketId);
    Task<(bool Success, string? ErrorMessage)> DeleteWorkLogAsync(int workLogId, int currentUserId);
}
