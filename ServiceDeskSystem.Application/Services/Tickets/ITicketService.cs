using ServiceDeskSystem.Application.Common.Models;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Application.Services.Tickets;

public interface ITicketService
{
    Task<List<Ticket>> GetAllTicketsAsync();

    Task<PagedResult<Ticket>> GetPagedTicketsAsync(int page, int pageSize);

    Task<Ticket?> GetTicketByIdAsync(int id);

    /// <summary>Status changes of the ticket, oldest first, with the user who made each change (none for backfilled rows).</summary>
    Task<List<TicketStatusHistory>> GetStatusHistoryAsync(int ticketId);

    Task<Ticket> CreateTicketAsync(Ticket ticket);

    // Edits take the ticket's Version the caller loaded; when the stored ticket has moved on since, nothing is
    // saved and the result is Conflict. Without a version the edit is saved whatever changed in between.

    /// <param name="actorUserId">The user making the change (recorded in history, audit and notifications).</param>
    Task<TicketUpdateResult> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, int? actorUserId, int? expectedVersion = null);

    Task<TicketUpdateResult> UpdateTicketPriorityAsync(int ticketId, TicketPriority newPriority, int? expectedVersion = null);

    Task<TicketUpdateResult> UpdateTicketDatesAsync(int ticketId, DateTime? startDate, DateTime? dueDate, int? actorUserId = null, int? expectedVersion = null);

    Task<TicketUpdateResult> UpdateAnalyticalNoteAsync(int ticketId, string? analyticalNote, int? actorUserId = null, int? expectedVersion = null);

    Task<bool> DeleteTicketAsync(int ticketId);

    Task<List<Product>> GetProductsAsync();

    Task<List<Ticket>> GetUserTicketsAsync(int userId);

    Task<List<Ticket>> GetDeveloperTicketsAsync(int developerId);
}

