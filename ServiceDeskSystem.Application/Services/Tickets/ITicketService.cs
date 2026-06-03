using ServiceDeskSystem.Application.Common.Models;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Application.Services.Tickets;

public interface ITicketService
{
    Task<List<Ticket>> GetAllTicketsAsync();

    Task<PagedResult<Ticket>> GetPagedTicketsAsync(int page, int pageSize);

    Task<Ticket?> GetTicketByIdAsync(int id);

    Task<Ticket> CreateTicketAsync(Ticket ticket);

    Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus);

    Task<bool> UpdateTicketDatesAsync(int ticketId, DateTime? startDate, DateTime? dueDate, int? actorUserId = null);

    Task<bool> DeleteTicketAsync(int ticketId);

    Task<List<Product>> GetProductsAsync();

    Task<List<Ticket>> GetUserTicketsAsync(int userId);

    Task<List<Ticket>> GetDeveloperTicketsAsync(int developerId);
}

