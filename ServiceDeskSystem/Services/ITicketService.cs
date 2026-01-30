using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services;

internal interface ITicketService
{
    Task<List<Ticket>> GetAllTicketsAsync();

    Task<Ticket?> GetTicketByIdAsync(int id);

    Task<Ticket> CreateTicketAsync(Ticket ticket);

    Task<Comment> AddCommentAsync(Comment comment);

    Task<bool> UpdateTicketStatusAsync(int ticketId, string newStatus);

    Task<List<Product>> GetProductsAsync();

    Task<int> GetTotalTicketsCountAsync();

    Task<int> GetOpenTicketsCountAsync();

    Task<int> GetCriticalTicketsCountAsync();

    Task<int> GetUserTicketsCountAsync(int userId);
}
