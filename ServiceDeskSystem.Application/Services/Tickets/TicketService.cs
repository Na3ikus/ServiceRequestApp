using ServiceDeskSystem.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Domain.Constants;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Application.Services.Tickets.Interfaces;

namespace ServiceDeskSystem.Application.Services.Tickets;

public sealed class TicketService(IDbContextFactory<BugTrackerDbContext> contextFactory)
    : ITicketService, ITicketAssignmentService, ITicketStatisticsService
{
    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets.ToList();
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
        ticket.Status = TicketConstants.Statuses.Open;

        if (string.IsNullOrWhiteSpace(ticket.Type))
        {
            ticket.Type = TicketConstants.Types.Support;
        }

        if (ticket.Type != TicketConstants.Types.Project && !ticket.ProductId.HasValue)
        {
            throw new ArgumentException("Product is required for non-project tickets.", nameof(ticket));
        }

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
        var tickets = await repo.Tickets.FindAsync(t => t.Status == TicketConstants.Statuses.Open).ConfigureAwait(false);
        return tickets.Count();
    }

    public async Task<int> GetCriticalTicketsCountAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.FindAsync(t => t.Priority == TicketConstants.Priorities.Critical).ConfigureAwait(false);
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

    public async Task<Dictionary<string, int>> GetTicketCountByStatusAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.GetAllAsync().ConfigureAwait(false);
        return tickets
            .GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetTicketCountByPriorityAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.GetAllAsync().ConfigureAwait(false);
        return tickets
            .GroupBy(t => t.Priority)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<List<(string Login, int Count)>> GetTopDevelopersAsync(int top = 5)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets
            .Where(t => t.Developer != null && (t.Status == "Resolved" || t.Status == "Closed" || t.Status == "Done"))
            .GroupBy(t => t.Developer!.Login ?? "?")
            .Select(g => (Login: g.Key, Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }
}

