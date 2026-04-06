namespace ServiceDeskSystem.Application.Services.Realtime.Interfaces;

public interface IRealtimeNotifier
{
    Task NotifyTicketsChangedAsync(CancellationToken cancellationToken = default);

    Task NotifyNotificationsChangedAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
}
