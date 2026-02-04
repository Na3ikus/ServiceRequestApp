using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Data.Repository;

namespace ServiceDeskSystem.Services.Tickets;

internal sealed class TicketService(IDbContextFactory<BugTrackerDbContext> contextFactory): ITicketService
{
    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets.ToList();
    }

    public async Task<Comment?> UpdateCommentAsync(int commentId, string newMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newMessage);

        await using var repo = new RepositoryFacade(contextFactory);
        var existing = await repo.Comments.GetByIdWithAuthorAsync(commentId).ConfigureAwait(false);

        if (existing is null)
        {
            return null;
        }

        existing.Message = newMessage;
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return existing;
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        return await repo.Tickets.GetByIdWithIncludesAsync(id).ConfigureAwait(false);
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        await using var repo = new RepositoryFacade(contextFactory);
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = "Open";

        await repo.Tickets.CreateAsync(ticket).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);

        return ticket;
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        await using var repo = new RepositoryFacade(contextFactory);
        comment.CreatedAt = DateTime.UtcNow;

        await repo.Comments.CreateAsync(comment).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);

        var result = await repo.Comments.GetByIdWithAuthorAsync(comment.Id).ConfigureAwait(false);
        return result ?? comment;
    }

    public async Task<bool> UpdateTicketStatusAsync(int ticketId, string newStatus)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.Status = newStatus;
        await repo.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeleteTicketAsync(int ticketId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var ticket = await repo.Tickets.GetByIdWithIncludesAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        await repo.Tickets.DeleteAsync(ticketId).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var products = await repo.Products.GetAllAsync().ConfigureAwait(false);
        return products.OrderBy(p => p.Name).ToList();
    }

    public async Task<int> GetTotalTicketsCountAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.GetAllAsync().ConfigureAwait(false);
        return tickets.Count();
    }

    public async Task<int> GetOpenTicketsCountAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.FindAsync(t => t.Status == "Open").ConfigureAwait(false);
        return tickets.Count();
    }

    public async Task<int> GetCriticalTicketsCountAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.FindAsync(t => t.Priority == "Critical").ConfigureAwait(false);
        return tickets.Count();
    }

    public async Task<int> GetUserTicketsCountAsync(int userId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.FindAsync(t => t.AuthorId == userId).ConfigureAwait(false);
        return tickets.Count();
    }

    public async Task<List<Ticket>> GetUserTicketsAsync(int userId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets.Where(t => t.AuthorId == userId).ToList();
    }

    public async Task<bool> AssignDeveloperAsync(int ticketId, int developerId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.DeveloperId = developerId;
        await repo.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> UnassignDeveloperAsync(int ticketId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.DeveloperId = null;
        await repo.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<List<Ticket>> GetDeveloperTicketsAsync(int developerId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.GetByDeveloperIdAsync(developerId).ConfigureAwait(false);
        return tickets.ToList();
    }

    public async Task<int> GetDeveloperAssignedCountAsync(int developerId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.FindAsync(t => t.DeveloperId == developerId).ConfigureAwait(false);
        return tickets.Count();
    }

    public async Task<int> GetDeveloperInProgressCountAsync(int developerId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.FindAsync(t => t.DeveloperId == developerId && t.Status == "In Progress").ConfigureAwait(false);
        return tickets.Count();
    }

    public async Task<int> GetDeveloperCompletedCountAsync(int developerId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.FindAsync(t => t.DeveloperId == developerId && (t.Status == "Resolved" || t.Status == "Closed")).ConfigureAwait(false);
        return tickets.Count();
    }
}
