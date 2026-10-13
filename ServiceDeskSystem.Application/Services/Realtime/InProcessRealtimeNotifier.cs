namespace ServiceDeskSystem.Application.Services.Realtime;

/// <summary>
/// Delivers change notifications to the pages open on this server through <see cref="IRealtimeEvents"/>.
/// Pages are Blazor Server circuits in the same process, so no network hop or hub is needed, and nobody
/// outside the process can subscribe to another user's notifications.
/// </summary>
public sealed class InProcessRealtimeNotifier : IRealtimeNotifier, IRealtimeEvents
{
    public event EventHandler? TicketsChanged;

    public event EventHandler<NotificationsChangedEventArgs>? NotificationsChanged;

    public event EventHandler? AuditLogsChanged;

    public Task NotifyTicketsChangedAsync(CancellationToken cancellationToken = default)
    {
        Raise(this.TicketsChanged, this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task NotifyNotificationsChangedAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var ids = userIds.Where(id => id > 0).Distinct().ToList();
        if (ids.Count > 0)
        {
            Raise(this.NotificationsChanged, this, new NotificationsChangedEventArgs(ids));
        }

        return Task.CompletedTask;
    }

    public Task NotifyAuditLogsChangedAsync(CancellationToken cancellationToken = default)
    {
        Raise(this.AuditLogsChanged, this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    // Each handler is a page; one that throws (e.g. its circuit is going away) must not fail the change
    // that raised the event or keep the other pages from hearing about it.
    private static void Raise(Delegate? handlers, object sender, EventArgs args)
    {
        foreach (var handler in handlers?.GetInvocationList() ?? [])
        {
            try
            {
                handler.DynamicInvoke(sender, args);
            }
            catch
            {
                // See above.
            }
        }
    }
}
