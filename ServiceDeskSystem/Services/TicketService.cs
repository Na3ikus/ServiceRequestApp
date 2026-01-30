using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services;

internal sealed class TicketService(IDbContextFactory<BugTrackerDbContext> contextFactory) : ITicketService
{
    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        return await dbContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Product)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        return await dbContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Product)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = "Open";

        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return ticket;
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        comment.CreatedAt = DateTime.UtcNow;

        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        await dbContext.Entry(comment)
            .Reference(c => c.Author)
            .LoadAsync()
            .ConfigureAwait(false);

        return comment;
    }

    public async Task<bool> UpdateTicketStatusAsync(int ticketId, string newStatus)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        var ticket = await dbContext.Tickets.FindAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.Status = newStatus;
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        return await dbContext.Products
            .OrderBy(p => p.Name)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<int> GetTotalTicketsCountAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        return await dbContext.Tickets.CountAsync().ConfigureAwait(false);
    }

    public async Task<int> GetOpenTicketsCountAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        return await dbContext.Tickets.CountAsync(t => t.Status == "Open").ConfigureAwait(false);
    }

    public async Task<int> GetCriticalTicketsCountAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        return await dbContext.Tickets.CountAsync(t => t.Priority == "Critical").ConfigureAwait(false);
    }

    public async Task<int> GetUserTicketsCountAsync(int userId)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        return await dbContext.Tickets.CountAsync(t => t.AuthorId == userId).ConfigureAwait(false);
    }
}
