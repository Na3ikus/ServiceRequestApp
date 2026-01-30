using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services;

public class TicketService(IDbContextFactory<BugTrackerDbContext> contextFactory) : ITicketService
{
    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        return await dbContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Product)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        return await dbContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Product)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = "Open";
        
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();
        
        return ticket;
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        comment.CreatedAt = DateTime.UtcNow;
        
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();
        
        // Reload the comment with author
        await dbContext.Entry(comment)
            .Reference(c => c.Author)
            .LoadAsync();
        
        return comment;
    }

    public async Task<bool> UpdateTicketStatusAsync(int ticketId, string newStatus)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        var ticket = await dbContext.Tickets.FindAsync(ticketId);
        
        if (ticket is null)
            return false;
        
        ticket.Status = newStatus;
        await dbContext.SaveChangesAsync();
        
        return true;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        return await dbContext.Products
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<int> GetTotalTicketsCountAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        return await dbContext.Tickets.CountAsync();
    }

    public async Task<int> GetOpenTicketsCountAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        return await dbContext.Tickets.CountAsync(t => t.Status == "Open");
    }

    public async Task<int> GetCriticalTicketsCountAsync()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        return await dbContext.Tickets.CountAsync(t => t.Priority == "Critical");
    }

    public async Task<int> GetUserTicketsCountAsync(int userId)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        return await dbContext.Tickets.CountAsync(t => t.AuthorId == userId);
    }
}
