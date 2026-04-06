using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Notifications.Models;

namespace ServiceDeskSystem.Components.Layout;

public partial class MainLayout
{
    private async Task ToggleNotificationsAsync()
    {
        this.isNotificationsOpen = !this.isNotificationsOpen;
        this.StopNotificationPulse();

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

    [JSInvokable]
    public async Task HandleOutsideNotificationClick()
    {
        if (!this.isNotificationsOpen)
        {
            return;
        }

        this.isNotificationsOpen = false;
        this.StopNotificationPulse();
        await this.UnregisterNotificationOutsideClickAsync();
        await this.InvokeAsync(this.StateHasChanged);
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

    private async Task StartNotificationsHubAsync()
    {
        if (this.notificationsHubInitialized)
        {
            return;
        }

        this.notificationsHubConnection = new HubConnectionBuilder()
            .WithUrl(this.Navigation.ToAbsoluteUri("/hubs/updates"))
            .WithAutomaticReconnect()
            .Build();

        this.notificationsHubConnection.On("NotificationsChanged", async () =>
        {
            await this.HandleNotificationsChangedAsync();
        });

        this.notificationsHubConnection.Reconnected += async _ =>
        {
            await this.JoinNotificationsGroupAsync().ConfigureAwait(false);
        };

        try
        {
            await this.notificationsHubConnection.StartAsync();
            await this.JoinNotificationsGroupAsync();
            this.notificationsHubInitialized = true;
        }
        catch
        {
            this.notificationsHubConnection = null;
        }
    }

    private async Task StopNotificationsHubAsync()
    {
        if (this.notificationsHubConnection is null)
        {
            return;
        }

        try
        {
            await this.LeaveNotificationsGroupAsync();
            await this.notificationsHubConnection.StopAsync();
            await this.notificationsHubConnection.DisposeAsync();
        }
        catch
        {
            // Ignore disposal/reconnect race issues.
        }
        finally
        {
            this.notificationsHubConnection = null;
            this.notificationsHubInitialized = false;
        }
    }

    private async Task HandleNotificationsChangedAsync()
    {
        if (!this.AuthService.IsAuthenticated)
        {
            return;
        }

        await this.InvokeAsync(async () =>
        {
            var previousUnread = this.lastKnownUnreadCount;
            await this.LoadNotificationsAsync();

            if (this.UnreadNotificationsCount > previousUnread)
            {
                this.StartNotificationPulse();
                await this.PlayNotificationSoundAsync();
            }

            this.StateHasChanged();
        });
    }

    private async Task JoinNotificationsGroupAsync()
    {
        var userId = this.AuthService.CurrentUser?.Id;
        if (userId is null || this.notificationsHubConnection is null)
        {
            return;
        }

        await this.notificationsHubConnection.InvokeAsync("JoinNotificationsGroup", userId.Value);
    }

    private async Task LeaveNotificationsGroupAsync()
    {
        var userId = this.AuthService.CurrentUser?.Id;
        if (userId is null || this.notificationsHubConnection is null)
        {
            return;
        }

        await this.notificationsHubConnection.InvokeAsync("LeaveNotificationsGroup", userId.Value);
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

    private void StartNotificationPulse()
    {
        this.StopNotificationPulse(clearState: false);
        this.hasNewNotificationPulse = true;

        this.notificationPulseCts = new CancellationTokenSource();
        var token = this.notificationPulseCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(NotificationPulseDuration, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                await this.InvokeAsync(() =>
                {
                    this.hasNewNotificationPulse = false;
                    this.StateHasChanged();
                });
            }
            catch
            {
                // Ignore UI updates during disposal.
            }
        });
    }

    private void StopNotificationPulse(bool clearState = true)
    {
        this.notificationPulseCts?.Cancel();
        this.notificationPulseCts?.Dispose();
        this.notificationPulseCts = null;

        if (clearState)
        {
            this.hasNewNotificationPulse = false;
        }
    }

    private async Task PlayNotificationSoundAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("notificationManager.playNewNotificationSound");
        }
        catch
        {
            // Ignore if browser blocks autoplay.
        }
    }
}
