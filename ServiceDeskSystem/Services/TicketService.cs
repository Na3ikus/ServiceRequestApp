using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services;

public class TicketService(BugTrackerDbContext dbContext) : ITicketService
{
    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        return await dbContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Product)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        return await dbContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Product)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = "Open";
        
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();
        
        return ticket;
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
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
        var ticket = await dbContext.Tickets.FindAsync(ticketId);
        
        if (ticket is null)
            return false;
        
        ticket.Status = newStatus;
        await dbContext.SaveChangesAsync();
        
        return true;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        return await dbContext.Products
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
