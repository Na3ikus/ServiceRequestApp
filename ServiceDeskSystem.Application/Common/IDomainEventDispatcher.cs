using ServiceDeskSystem.Domain.Common;

namespace ServiceDeskSystem.Application.Common;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
