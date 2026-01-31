using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services.Tickets;

internal sealed class TicketService(IDbContextFactory<BugTrackerDbContext> contextFactory): ITicketService
{
    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Tickets
                .Include(t => t.Author)
                .Include(t => t.Product)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }

    public async Task<Comment?> UpdateCommentAsync(int commentId, string newMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newMessage);

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var existing = await dbContext.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId)
                .ConfigureAwait(false);

            if (existing is null)
            {
                return null;
            }

            existing.Message = newMessage;
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return existing;
        }
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Tickets
                .Include(t => t.Author)
                .Include(t => t.Product)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);
        }
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.Status = "Open";

            dbContext.Tickets.Add(ticket);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return ticket;
        }
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            comment.CreatedAt = DateTime.UtcNow;

            dbContext.Comments.Add(comment);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            await dbContext.Entry(comment)
                .Reference(c => c.Author)
                .LoadAsync()
                .ConfigureAwait(false);

            return comment;
        }
    }

    public async Task<bool> UpdateTicketStatusAsync(int ticketId, string newStatus)
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var ticket = await dbContext.Tickets.FindAsync(ticketId).ConfigureAwait(false);

            if (ticket is null)
            {
                return false;
            }

            ticket.Status = newStatus;
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }
    }

    public async Task<bool> DeleteTicketAsync(int ticketId)
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var ticket = await dbContext.Tickets
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                .ConfigureAwait(false);

            if (ticket is null)
            {
                return false;
            }

            dbContext.Tickets.Remove(ticket);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Products
                .OrderBy(p => p.Name)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }

    public async Task<int> GetTotalTicketsCountAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Tickets.CountAsync().ConfigureAwait(false);
        }
    }

    public async Task<int> GetOpenTicketsCountAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Tickets.CountAsync(t => t.Status == "Open").ConfigureAwait(false);
        }
    }

    public async Task<int> GetCriticalTicketsCountAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Tickets.CountAsync(t => t.Priority == "Critical").ConfigureAwait(false);
        }
    }

    public async Task<int> GetUserTicketsCountAsync(int userId)
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Tickets.CountAsync(t => t.AuthorId == userId).ConfigureAwait(false);
        }
    }
}
