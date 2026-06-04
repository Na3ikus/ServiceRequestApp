
using ServiceDeskSystem.Application.Common.Models;
using ServiceDeskSystem.Domain.Constants;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Application.Services.Audit;

namespace ServiceDeskSystem.Application.Services.Tickets;

public sealed class TicketService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    INotificationService notificationService,
    IRealtimeNotifier realtimeNotifier,
    ServiceDeskSystem.Application.Common.IDomainEventDispatcher domainEventDispatcher,
    IAuditService? auditService = null)
    : ITicketService, ITicketAssignmentService, ITicketStatisticsService
{


    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets.ToList();
    }


    public async Task<PagedResult<Ticket>> GetPagedTicketsAsync(int page, int pageSize)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var (items, totalCount) = await repo.Tickets.GetPagedWithIncludesAsync(page, pageSize).ConfigureAwait(false);
        return new PagedResult<Ticket>(items.ToList(), totalCount, page, pageSize);
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
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetTotalCountAsync().ConfigureAwait(false);
    }

    public async Task<int> GetOpenTicketsCountAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetCountByStatusAsync(TicketStatus.Open).ConfigureAwait(false);
    }

    public async Task<int> GetCriticalTicketsCountAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetCountByPriorityAsync(TicketPriority.Critical).ConfigureAwait(false);
    }

    public async Task<int> GetUserTicketsCountAsync(int userId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetCountByAuthorIdAsync(userId).ConfigureAwait(false);
    }

    public async Task<List<Ticket>> GetUserTicketsAsync(int userId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets.Where(t => t.AuthorId == userId).OrderByDescending(t => t.CreatedAt).ToList();
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
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetCountByDeveloperIdAsync(developerId).ConfigureAwait(false);
    }

    public async Task<int> GetDeveloperInProgressCountAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetDeveloperInProgressCountAsync(developerId).ConfigureAwait(false);
    }

    public async Task<int> GetDeveloperCompletedCountAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetDeveloperCompletedCountAsync(developerId).ConfigureAwait(false);
    }

    public async Task<DeveloperDashboardStatsDto> GetDeveloperDashboardStatsAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var total = await repo.Tickets.GetCountByDeveloperIdAsync(developerId).ConfigureAwait(false);
        var inProgress = await repo.Tickets.GetDeveloperInProgressCountAsync(developerId).ConfigureAwait(false);
        var completed = await repo.Tickets.GetDeveloperCompletedCountAsync(developerId).ConfigureAwait(false);
        return new DeveloperDashboardStatsDto(total, inProgress, completed);
    }

    public async Task<Dictionary<string, int>> GetTicketCountByStatusAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var counts = await repo.Tickets.GetTicketCountGroupedByStatusAsync().ConfigureAwait(false);
        return counts.ToDictionary(k => k.Key.ToString(), v => v.Value);
    }

    public async Task<Dictionary<string, int>> GetTicketCountByPriorityAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var counts = await repo.Tickets.GetTicketCountGroupedByPriorityAsync().ConfigureAwait(false);
        return counts.ToDictionary(k => k.Key.ToString(), v => v.Value);
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


