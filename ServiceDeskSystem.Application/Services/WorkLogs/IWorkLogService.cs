using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.WorkLogs;

public interface IWorkLogService
{
    /// <param name="localDay">The day the user picked in their own zone; shown in the audit log (defaults to the date of <paramref name="dateLogged"/>).</param>
    Task<(bool Success, string? ErrorMessage)> AddWorkLogAsync(int ticketId, int userId, int timeSpentMinutes, DateTime dateLogged, string description, DateOnly? localDay = null);
    Task<IEnumerable<WorkLog>> GetWorkLogsByTicketIdAsync(int ticketId);
    Task<IEnumerable<WorkLog>> GetWorkLogsByUserIdAsync(int userId);
    Task<int> GetTotalTimeSpentForTicketAsync(int ticketId);
    Task<int> GetTotalTimeSpentForUserAsync(int userId);
    Task<(bool Success, string? ErrorMessage)> DeleteWorkLogAsync(int workLogId, int currentUserId);
}
