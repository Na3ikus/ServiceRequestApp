using ServiceDeskSystem.Domain.Common;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Domain.Events;

public record TicketStatusChangedEvent(int TicketId, TicketStatus OldStatus, TicketStatus NewStatus, int? ActorUserId) : IDomainEvent;
