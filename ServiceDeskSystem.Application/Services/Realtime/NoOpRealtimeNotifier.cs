using ServiceDeskSystem.Application.Services.Realtime;

namespace ServiceDeskSystem.Application.Services.Realtime;

public sealed class NoOpRealtimeNotifier : IRealtimeNotifier
{
    public static NoOpRealtimeNotifier Instance { get; } = new();

    private NoOpRealtimeNotifier()
    {
    }

    public Task NotifyTicketsChangedAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyNotificationsChangedAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

