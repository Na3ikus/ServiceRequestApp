using Microsoft.AspNetCore.SignalR;

namespace ServiceDeskSystem.Hubs;

public sealed class UpdatesHub : Hub
{
    public static string GetUserNotificationsGroupName(int userId) => $"notifications:{userId}";

    public Task JoinNotificationsGroup(int userId)
    {
        if (userId <= 0)
        {
            return Task.CompletedTask;
        }

        return this.Groups.AddToGroupAsync(this.Context.ConnectionId, GetUserNotificationsGroupName(userId));
    }

    public Task LeaveNotificationsGroup(int userId)
    {
        if (userId <= 0)
        {
            return Task.CompletedTask;
        }

        return this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, GetUserNotificationsGroupName(userId));
    }
}
