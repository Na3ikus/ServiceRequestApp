using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Infrastructure.Data.Repository;
using ServiceDeskSystem.Domain.Constants;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications.Interfaces;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Realtime.Interfaces;
using ServiceDeskSystem.Application.Services.Tickets.Interfaces;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Application.Services.Audit.Interfaces;

namespace ServiceDeskSystem.Application.Services.Tickets;

public sealed class TicketService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    IDbContextFactory<BugTrackerDbContext> contextFactory,
    INotificationService notificationService,
    IRealtimeNotifier realtimeNotifier,
    ServiceDeskSystem.Application.Common.IDomainEventDispatcher domainEventDispatcher,
    IAuditService? auditService = null)
    : ITicketService, ITicketAssignmentService, ITicketStatisticsService
{
    public TicketService(
        IDbContextFactory<BugTrackerDbContext> contextFactory,
        INotificationService notificationService,
        ServiceDeskSystem.Application.Common.IDomainEventDispatcher domainEventDispatcher)
        : this(new RepositoryFacadeFactory(contextFactory), contextFactory, notificationService, NoOpRealtimeNotifier.Instance, domainEventDispatcher, null)
    {
    }

    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets.ToList();
    }


    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetByIdWithIncludesAsync(id).ConfigureAwait(false);
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        await using var repo = repositoryFacadeFactory.Create();
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = TicketStatus.Open;

        if (ticket.Type != TicketType.Project && !ticket.ProductId.HasValue)
        {
            throw new ArgumentException("Product is required for non-project tickets.", nameof(ticket));
        }

        ticket.AddDomainEvent(new ServiceDeskSystem.Domain.Events.TicketCreatedEvent(0, ticket.AuthorId, ticket.Title));

        await repo.Tickets.CreateAsync(ticket).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        
        var events = ticket.DomainEvents.ToList();
        for (int i = 0; i < events.Count; i++)
        {
            if (events[i] is ServiceDeskSystem.Domain.Events.TicketCreatedEvent createdEvent && createdEvent.TicketId == 0)
            {
                events[i] = createdEvent with { TicketId = ticket.Id };
            }
        }
        await domainEventDispatcher.DispatchAsync(events).ConfigureAwait(false);
        ticket.ClearDomainEvents();

        return ticket;
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        await using var repo = repositoryFacadeFactory.Create();
        comment.CreatedAt = DateTime.UtcNow;

        await repo.Comments.CreateAsync(comment).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        var result = await repo.Comments.GetByIdWithAuthorAsync(comment.Id).ConfigureAwait(false);
        return result ?? comment;
    }

    public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        var oldStatus = ticket.Status;
        ticket.ChangeStatus(newStatus, ticket.DeveloperId); // Use Domain Entity method
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await domainEventDispatcher.DispatchAsync(ticket.DomainEvents).ConfigureAwait(false);
        ticket.ClearDomainEvents();

        // The remaining code (notification, audit, realtimeNotifier) is handled by DomainEvent handlers.
        // But what if oldStatus == newStatus? The DomainEvents collection will be empty, so DispatchAsync does nothing.

        return true;
    }

    public async Task<bool> UpdateTicketDatesAsync(int ticketId, DateTime? startDate, DateTime? dueDate, int? actorUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        bool datesChanged = ticket.StartDate != startDate || ticket.DueDate != dueDate;

        ticket.StartDate = startDate;
        ticket.DueDate = dueDate;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        if (datesChanged)
        {
            await notificationService.CreateDatesChangedNotificationAsync(ticketId, actorUserId).ConfigureAwait(false);
        }

        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeleteTicketAsync(int ticketId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdWithIncludesAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        await repo.Tickets.DeleteAsync(ticketId).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);
        
        await auditService.LogActionSafeAsync("DELETE", "Ticket", ticket.Id.ToString(), $"Deleted ticket: {ticket.Title}").ConfigureAwait(false);
        
        return true;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var products = await repo.Products.GetAllAsync().ConfigureAwait(false);
        return products.OrderBy(p => p.Name).ToList();
    }

    public async Task<int> GetTotalTicketsCountAsync()
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets.AsNoTracking().CountAsync().ConfigureAwait(false);
    }

    public async Task<int> GetOpenTicketsCountAsync()
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.Status == TicketStatus.Open)
            .ConfigureAwait(false);
    }

    public async Task<int> GetCriticalTicketsCountAsync()
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.Priority == TicketPriority.Critical)
            .ConfigureAwait(false);
    }

    public async Task<int> GetUserTicketsCountAsync(int userId)
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.AuthorId == userId)
            .ConfigureAwait(false);
    }

    public async Task<List<Ticket>> GetUserTicketsAsync(int userId)
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .Include(t => t.Author)
            .Include(t => t.Product)
            .Include(t => t.Developer)
            .Where(t => t.AuthorId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> AssignDeveloperAsync(int ticketId, int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.DeveloperId = developerId;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> UnassignDeveloperAsync(int ticketId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.DeveloperId = null;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<List<Ticket>> GetDeveloperTicketsAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetByDeveloperIdAsync(developerId).ConfigureAwait(false);
        return tickets.ToList();
    }

    public async Task<int> GetDeveloperAssignedCountAsync(int developerId)
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.DeveloperId == developerId)
            .ConfigureAwait(false);
    }

    public async Task<int> GetDeveloperInProgressCountAsync(int developerId)
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.DeveloperId == developerId && t.Status == TicketStatus.InProgress)
            .ConfigureAwait(false);
    }

    public async Task<int> GetDeveloperCompletedCountAsync(int developerId)
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.DeveloperId == developerId &&
                             (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed))
            .ConfigureAwait(false);
    }

    public async Task<DeveloperDashboardStatsDto> GetDeveloperDashboardStatsAsync(int developerId)
    {
        await using var context = contextFactory.CreateDbContext();

        var stats = await context.Tickets
            .AsNoTracking()
            .Where(t => t.DeveloperId == developerId)
            .GroupBy(_ => 1)
            .Select(g => new DeveloperDashboardStatsDto(
                g.Count(),
                g.Count(t => t.Status == TicketStatus.InProgress),
                g.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)))
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);

        return stats ?? new DeveloperDashboardStatsDto(0, 0, 0);
    }

    public async Task<Dictionary<string, int>> GetTicketCountByStatusAsync()
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .GroupBy(t => t.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key.ToString(), x => x.Count)
            .ConfigureAwait(false);
    }

    public async Task<Dictionary<string, int>> GetTicketCountByPriorityAsync()
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.Tickets
            .AsNoTracking()
            .GroupBy(t => t.Priority)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key.ToString(), x => x.Count)
            .ConfigureAwait(false);
    }

    public async Task<List<(string Login, int Count)>> GetTopDevelopersAsync(int top = 5)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets
            .Where(t => t.Developer != null && (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.Done))
            .GroupBy(t => t.Developer!.Login ?? "?")
            .Select(g => (Login: g.Key, Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }
}

