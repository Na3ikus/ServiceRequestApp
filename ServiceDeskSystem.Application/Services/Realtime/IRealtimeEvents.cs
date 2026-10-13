namespace ServiceDeskSystem.Application.Services.Realtime;

/// <summary>
/// Change events raised in this process for open pages to refresh. Handlers run on the thread that made the change,
/// so components must switch to their dispatcher (<c>InvokeAsync</c>) before touching state.
/// </summary>
public interface IRealtimeEvents
{
    event EventHandler? TicketsChanged;

    event EventHandler<NotificationsChangedEventArgs>? NotificationsChanged;

    event EventHandler? AuditLogsChanged;
}
