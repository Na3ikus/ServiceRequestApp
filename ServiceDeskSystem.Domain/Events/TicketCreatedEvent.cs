using ServiceDeskSystem.Domain.Common;

namespace ServiceDeskSystem.Domain.Events;

public record TicketCreatedEvent(int TicketId, int AuthorId, string Title) : IDomainEvent;
