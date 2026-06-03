using MediatR;
using ServiceDeskSystem.Application.Common;
using ServiceDeskSystem.Application.Services.Audit.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications.Interfaces;
using ServiceDeskSystem.Application.Services.Realtime.Interfaces;
using ServiceDeskSystem.Domain.Events;

namespace ServiceDeskSystem.Application.EventHandlers;

public class TicketStatusChangedEventHandler : INotificationHandler<DomainEventNotification<TicketStatusChangedEvent>>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public TicketStatusChangedEventHandler(
        INotificationService notificationService,
        IAuditService auditService,
        IRealtimeNotifier realtimeNotifier)
    {
        _notificationService = notificationService;
        _auditService = auditService;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task Handle(DomainEventNotification<TicketStatusChangedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        await _notificationService.CreateStatusChangedNotificationAsync(
            domainEvent.TicketId, 
            domainEvent.OldStatus, 
            domainEvent.NewStatus, 
            domainEvent.ActorUserId);

        await _auditService.LogActionSafeAsync(
            "STATUS_UPDATE", 
            "Ticket", 
            domainEvent.TicketId.ToString(), 
            $"Status changed from {domainEvent.OldStatus} to {domainEvent.NewStatus}", 
            domainEvent.ActorUserId);

        await _realtimeNotifier.NotifyTicketsChangedAsync();
    }
}
