namespace ServiceDeskSystem.Application.Services.Realtime;

/// <summary>
/// The users whose notifications changed; only their pages need to reload.
/// </summary>
public sealed class NotificationsChangedEventArgs(IReadOnlyCollection<int> userIds) : EventArgs
{
    public IReadOnlyCollection<int> UserIds { get; } = userIds;
}
