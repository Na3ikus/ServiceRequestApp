using MediatR;
using ServiceDeskSystem.Application.Common;
using ServiceDeskSystem.Application.Services.Audit.Interfaces;
using ServiceDeskSystem.Application.Services.Realtime.Interfaces;
using ServiceDeskSystem.Domain.Events;

namespace ServiceDeskSystem.Application.EventHandlers;

public class TicketCreatedEventHandler : INotificationHandler<DomainEventNotification<TicketCreatedEvent>>
{
    private readonly IAuditService _auditService;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public TicketCreatedEventHandler(
        IAuditService auditService,
        IRealtimeNotifier realtimeNotifier)
    {
        _auditService = auditService;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task Handle(DomainEventNotification<TicketCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await _auditService.LogActionSafeAsync(
            "CREATE", 
            "Ticket", 
            domainEvent.TicketId.ToString(), 
            $"Created ticket: {domainEvent.Title}", 
            domainEvent.AuthorId);

        await _realtimeNotifier.NotifyTicketsChangedAsync();
    }
}
