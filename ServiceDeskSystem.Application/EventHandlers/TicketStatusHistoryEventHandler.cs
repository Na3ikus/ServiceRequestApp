using MediatR;
using Microsoft.Extensions.Logging;
using ServiceDeskSystem.Application.Common;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Events;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.EventHandlers;

/// <summary>
/// Records every ticket status change in <see cref="TicketStatusHistory"/>, independent of the
/// (clearable) audit log.
/// </summary>
public class TicketStatusHistoryEventHandler : INotificationHandler<DomainEventNotification<TicketStatusChangedEvent>>
{
    private readonly IRepositoryFacadeFactory _repositoryFacadeFactory;
    private readonly ILogger<TicketStatusHistoryEventHandler> _logger;

    public TicketStatusHistoryEventHandler(
        IRepositoryFacadeFactory repositoryFacadeFactory,
        ILogger<TicketStatusHistoryEventHandler> logger)
    {
        _repositoryFacadeFactory = repositoryFacadeFactory;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<TicketStatusChangedEvent> notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var domainEvent = notification.DomainEvent;

        try
        {
            await using var repo = _repositoryFacadeFactory.Create();
            await repo.TicketStatusHistory.CreateAsync(new TicketStatusHistory
            {
                TicketId = domainEvent.TicketId,
                OldStatus = domainEvent.OldStatus,
                NewStatus = domainEvent.NewStatus,
                ChangedByUserId = domainEvent.ActorUserId,
                ChangedAt = DateTime.UtcNow,
            }).ConfigureAwait(false);
            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // The ticket itself is already saved; losing one history row must not break the request.
            _logger.LogError(ex, "Failed to record status history for ticket {TicketId}", domainEvent.TicketId);
        }
    }
}
