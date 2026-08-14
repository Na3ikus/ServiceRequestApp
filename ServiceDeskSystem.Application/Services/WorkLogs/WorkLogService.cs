using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.Services.WorkLogs;

public class WorkLogService : IWorkLogService
{
    private readonly IRepositoryFacadeFactory _repositoryFacadeFactory;
    private readonly IAuditService _auditService;

    public WorkLogService(IRepositoryFacadeFactory repositoryFacadeFactory, IAuditService auditService)
    {
        this._repositoryFacadeFactory = repositoryFacadeFactory;
        this._auditService = auditService;
    }

    public async Task<(bool Success, string? ErrorMessage)> AddWorkLogAsync(int ticketId, int userId, int timeSpentMinutes, DateTime dateLogged, string description)
    {
        if (timeSpentMinutes <= 0)
        {
            return (false, "Time spent must be greater than zero.");
        }

        await using var repo = this._repositoryFacadeFactory.Create();

        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);
        if (ticket is null)
        {
            return (false, "Ticket not found.");
        }

        var workLog = new WorkLog
        {
            TicketId = ticketId,
            UserId = userId,
            TimeSpentMinutes = timeSpentMinutes,
            DateLogged = dateLogged.ToUniversalTime(),
            Description = description
        };

        await repo.WorkLogs.CreateAsync(workLog).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await this._auditService.LogActionSafeAsync("WORK_LOG_ADDED", "Ticket", ticketId.ToString(), $"Added work log: {timeSpentMinutes} min on {dateLogged.ToShortDateString()}", userId).ConfigureAwait(false);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteWorkLogAsync(int workLogId, int currentUserId)
    {
        await using var repo = this._repositoryFacadeFactory.Create();

        var workLog = await repo.WorkLogs.GetByIdAsync(workLogId).ConfigureAwait(false);
        if (workLog is null)
        {
            return (false, "Work log not found.");
        }

        var user = await repo.Users.GetByIdAsync(currentUserId).ConfigureAwait(false);
        if (user is null)
        {
            return (false, "User not found.");
        }

        if (workLog.UserId != currentUserId && user.Role != Domain.Enums.UserRole.Admin)
        {
            return (false, "Forbidden: Only the author or an admin can delete a work log.");
        }

        await repo.WorkLogs.DeleteAsync(workLogId).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await this._auditService.LogActionSafeAsync("WORK_LOG_DELETED", "Ticket", workLog.TicketId.ToString(), $"Deleted work log for {workLog.TimeSpentMinutes} min", currentUserId).ConfigureAwait(false);

        return (true, null);
    }

    public async Task<int> GetTotalTimeSpentForTicketAsync(int ticketId)
    {
        await using var repo = this._repositoryFacadeFactory.Create();
        return await repo.WorkLogs.GetTotalTimeSpentForTicketAsync(ticketId).ConfigureAwait(false);
    }

    public async Task<IEnumerable<WorkLog>> GetWorkLogsByTicketIdAsync(int ticketId)
    {
        await using var repo = this._repositoryFacadeFactory.Create();
        return await repo.WorkLogs.GetByTicketIdAsync(ticketId).ConfigureAwait(false);
    }

    public async Task<IEnumerable<WorkLog>> GetWorkLogsByUserIdAsync(int userId)
    {
        await using var repo = this._repositoryFacadeFactory.Create();
        return await repo.WorkLogs.GetByUserIdAsync(userId).ConfigureAwait(false);
    }
}
