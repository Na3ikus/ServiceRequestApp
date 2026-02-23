using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Tickets.Interfaces;

public interface ITicketService
{
    Task<List<Ticket>> GetAllTicketsAsync();

    Task<Ticket?> GetTicketByIdAsync(int id);

    Task<Ticket> CreateTicketAsync(Ticket ticket);

    Task<bool> UpdateTicketStatusAsync(int ticketId, string newStatus);

    Task<bool> DeleteTicketAsync(int ticketId);

    Task<List<Product>> GetProductsAsync();

    Task<List<Ticket>> GetUserTicketsAsync(int userId);

    Task<List<Ticket>> GetDeveloperTicketsAsync(int developerId);
}
