using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Tickets.Interfaces;

/// <summary>
/// Provides ticket assignment operations.
/// Isolated from core CRUD to allow independent extension (e.g. notifications, workload checks).
/// </summary>
public interface ITicketAssignmentService
{
    Task<bool> AssignDeveloperAsync(int ticketId, int developerId);

    Task<bool> UnassignDeveloperAsync(int ticketId);
}
