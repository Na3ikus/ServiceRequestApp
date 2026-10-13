using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Tickets;

/// <summary>
/// Provides ticket assignment operations.
/// Isolated from core CRUD to allow independent extension (e.g. notifications, workload checks).
/// </summary>
public interface ITicketAssignmentService
{
    /// <summary>
    /// Assigns the developer; with <paramref name="expectedVersion"/> it is a Conflict when the ticket changed
    /// (e.g. someone else took it) after the caller loaded it. See <see cref="ITicketService"/>.
    /// </summary>
    Task<TicketUpdateResult> AssignDeveloperAsync(int ticketId, int developerId, int? expectedVersion = null);

    Task<TicketUpdateResult> UnassignDeveloperAsync(int ticketId, int? expectedVersion = null);
}

