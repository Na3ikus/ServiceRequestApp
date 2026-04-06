using Microsoft.AspNetCore.SignalR;
using ServiceDeskSystem.Application.Services.Realtime.Interfaces;
using ServiceDeskSystem.Hubs;

namespace ServiceDeskSystem.Services.Realtime;

public sealed class SignalRRealtimeNotifier(IHubContext<UpdatesHub> hubContext) : IRealtimeNotifier
{
    public async Task NotifyTicketsChangedAsync(CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.All.SendAsync("TicketsChanged", cancellationToken).ConfigureAwait(false);
    }

    public async Task NotifyNotificationsChangedAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return;
        }

        foreach (var userId in ids)
        {
            await hubContext.Clients
                .Group(UpdatesHub.GetUserNotificationsGroupName(userId))
                .SendAsync("NotificationsChanged", cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
