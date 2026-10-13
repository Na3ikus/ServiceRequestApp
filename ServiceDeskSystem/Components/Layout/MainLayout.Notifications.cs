using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Notifications.Models;
using ServiceDeskSystem.Application.Services.Realtime;

namespace ServiceDeskSystem.Components.Layout;

public partial class MainLayout
{
    [JSInvokable]
    public async Task HandleOutsideNotificationClick()
    {
        if (!this.isNotificationsOpen)
        {
            return;
        }

        this.isNotificationsOpen = false;
        await this.UnregisterNotificationOutsideClickAsync();
        await this.InvokeAsync(this.StateHasChanged);
    }

    private async Task ToggleNotificationsAsync()
    {
        this.isNotificationsOpen = !this.isNotificationsOpen;

        if (this.isNotificationsOpen)
        {
            await this.RegisterNotificationOutsideClickAsync();
            await this.LoadNotificationsAsync();

            var userId = this.AuthService.CurrentUser?.Id;
            if (userId is not null && this.UnreadNotificationsCount > 0)
            {
                await this.NotificationService.MarkAllAsReadAsync(userId.Value);
                await this.LoadNotificationsAsync();
            }
        }
        else
        {
            await this.UnregisterNotificationOutsideClickAsync();
        }
    }

    private async Task DeleteNotificationAsync(int notificationId)
    {
        var userId = this.AuthService.CurrentUser?.Id;
        if (userId is null)
        {
            return;
        }

        await this.NotificationService.DeleteAsync(notificationId, userId.Value);
        await this.LoadNotificationsAsync();
    }

    private async Task MarkNotificationsAsReadAsync()
    {
        var userId = this.AuthService.CurrentUser?.Id;
        if (userId is null)
        {
            return;
        }

        await this.NotificationService.MarkAllAsReadAsync(userId.Value);
        await this.LoadNotificationsAsync();
    }

    private async Task OpenNotificationAsync(UserNotificationDto notification)
    {
        this.isNotificationsOpen = false;
        this.Navigation.NavigateTo($"/ticket/{notification.TicketId}");
        await this.LoadNotificationsAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        var userId = this.AuthService.CurrentUser?.Id;
        if (userId is null)
        {
            this.notifications.Clear();
            this.UnreadNotificationsCount = 0;
            return;
        }

        var latest = await this.NotificationService.GetRecentForUserAsync(userId.Value, 8);
        this.notifications.Clear();
        this.notifications.AddRange(latest);
        this.UnreadNotificationsCount = await this.NotificationService.GetUnreadCountAsync(userId.Value);
        this.lastKnownUnreadCount = this.UnreadNotificationsCount;
    }

    // Raised on the thread of whoever changed the notifications; only this user's changes matter here.
    private void OnNotificationsChanged(object? sender, NotificationsChangedEventArgs e)
    {
        if (this.AuthService.CurrentUser?.Id is not int userId || !e.UserIds.Contains(userId))
        {
            return;
        }

        _ = this.HandleNotificationsChangedAsync();
    }

    private async Task HandleNotificationsChangedAsync()
    {
        try
        {
            await this.InvokeAsync(async () =>
            {
                if (this.disposed || !this.AuthService.IsAuthenticated)
                {
                    return;
                }

                var previousUnread = this.lastKnownUnreadCount;
                await this.LoadNotificationsAsync();

                if (this.UnreadNotificationsCount > previousUnread)
                {
                    var newest = this.notifications.FirstOrDefault();
                    await this.NotifyNewAsync(newest);

                    // Show visual toast notification for the newest received notification
                    if (newest != null)
                    {
                        await this.ToastService.ShowToastAsync(newest.Message, ToastType.Info);
                    }
                }

                this.StateHasChanged();
            });
        }
        catch
        {
            // The circuit may be closing; there is nothing left to update.
        }
    }

    private async Task RegisterNotificationOutsideClickAsync()
    {
        if (this.dotNetRef is null)
        {
            return;
        }

        try
        {
            await this.JS.InvokeVoidAsync("notificationManager.registerOutsideClick", this.dotNetRef);
        }
        catch
        {
            // Ignore JS interop issues during reconnect/shutdown.
        }
    }

    private async Task UnregisterNotificationOutsideClickAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("notificationManager.unregisterOutsideClick");
        }
        catch
        {
            // Ignore JS interop issues during reconnect/shutdown.
        }
    }

    private async Task NotifyNewAsync(UserNotificationDto? newest)
    {
        var title = newest != null ? $"ServiceDesk #{newest.TicketId}" : "ServiceDesk";
        var body = newest?.Message ?? string.Empty;

        try
        {
            // Sound and desktop notification are gated by user preferences in prefs.js.
            await this.JS.InvokeVoidAsync("notificationManager.notifyNew", title, body);
        }
        catch
        {
            // Ignore if browser blocks autoplay.
        }
    }
}
