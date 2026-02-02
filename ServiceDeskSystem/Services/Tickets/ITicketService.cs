using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services.Tickets;

internal interface ITicketService
{
    Task<List<Ticket>> GetAllTicketsAsync();

    Task<Ticket?> GetTicketByIdAsync(int id);

    Task<Ticket> CreateTicketAsync(Ticket ticket);

    Task<Comment> AddCommentAsync(Comment comment);

    Task<Comment?> UpdateCommentAsync(int commentId, string newMessage);

    Task<bool> UpdateTicketStatusAsync(int ticketId, string newStatus);

    Task<bool> DeleteTicketAsync(int ticketId);

    Task<List<Product>> GetProductsAsync();

    Task<int> GetTotalTicketsCountAsync();

    Task<int> GetOpenTicketsCountAsync();

    Task<int> GetCriticalTicketsCountAsync();

    Task<int> GetUserTicketsCountAsync(int userId);

    Task<bool> AssignDeveloperAsync(int ticketId, int developerId);

    Task<bool> UnassignDeveloperAsync(int ticketId);

    Task<List<Ticket>> GetDeveloperTicketsAsync(int developerId);

    Task<int> GetDeveloperAssignedCountAsync(int developerId);

    Task<int> GetDeveloperInProgressCountAsync(int developerId);

    Task<int> GetDeveloperCompletedCountAsync(int developerId);
}
