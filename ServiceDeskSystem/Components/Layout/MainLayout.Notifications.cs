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
        this.lastPolledUnreadCount = this.UnreadNotificationsCount;
    }

    private void StartNotificationMonitor()
    {
        if (this.notificationMonitorCts is not null)
        {
            return;
        }

        this.notificationMonitorCts = new CancellationTokenSource();
        this.notificationMonitorTask = this.MonitorNotificationsAsync(this.notificationMonitorCts.Token);
    }

    private async Task StopNotificationMonitorAsync()
    {
        if (this.notificationMonitorCts is null)
        {
            return;
        }

        await this.notificationMonitorCts.CancelAsync();

        if (this.notificationMonitorTask is not null)
        {
            try
            {
                await this.notificationMonitorTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown.
            }
        }

        this.notificationMonitorCts.Dispose();
        this.notificationMonitorCts = null;
        this.notificationMonitorTask = null;
    }

    private async Task MonitorNotificationsAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(NotificationMonitorInterval);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            if (!this.AuthService.IsAuthenticated)
            {
                continue;
            }

            var userId = this.AuthService.CurrentUser?.Id;
            if (userId is null)
            {
                continue;
            }

            int unreadCount;

            try
            {
                unreadCount = await this.NotificationService.GetUnreadCountAsync(userId.Value);
            }
            catch
            {
                continue;
            }

            if (this.lastPolledUnreadCount == unreadCount)
            {
                continue;
            }

            var hasIncrease = this.lastPolledUnreadCount.HasValue && unreadCount > this.lastPolledUnreadCount.Value;

            await this.InvokeAsync(async () =>
            {
                await this.LoadNotificationsAsync();

                if (hasIncrease)
                {
                    this.StartNotificationPulse();
                    await this.PlayNotificationSoundAsync();
                }

                this.StateHasChanged();
            });
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
