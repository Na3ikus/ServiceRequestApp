using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications.Models;
using ServiceDeskSystem.Application.Services.Theme.Interfaces;
using ServiceDeskSystem.Components.UI.Base;

namespace ServiceDeskSystem.Components.Layout;

/// <summary>
/// Main application layout with responsive sidebar and header actions.
/// </summary>
public partial class MainLayout : LayoutComponentBase, IDisposable, IAsyncDisposable
{
    private static readonly TimeSpan DatabaseMonitorInterval = TimeSpan.FromSeconds(15);
    private readonly List<ToastMessage> toasts = [];
    private readonly List<UserNotificationDto> notifications = [];

    private bool authRestored;
    private bool isNotificationsOpen;
    private bool isSidebarOpen;
    private bool isSidebarCollapsed;
    private bool hotkeyRegistered;
    private bool databaseConnectionLost;
    private bool disposed;
    private DotNetObjectReference<MainLayout>? dotNetRef;
    private CancellationTokenSource? databaseMonitorCts;
    private Task? databaseMonitorTask;

    internal IReadOnlyList<ToastMessage> Toasts => this.toasts;
    internal IReadOnlyList<UserNotificationDto> Notifications => this.notifications;
    internal int UnreadNotificationsCount { get; private set; }
    internal bool IsNotificationsOpen => this.isNotificationsOpen;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private ILocalizationService L { get; set; } = null!;

    [Inject]
    private IThemeService Theme { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; } = null!;

    [Inject]
    private INotificationService NotificationService { get; set; } = null!;

    [JSInvokable]
    public async Task HandleSidebarHotkey()
    {
        var isDesktop = await this.JS.InvokeAsync<bool>("sidebarManager.isDesktop");

        if (isDesktop)
        {
            await this.ToggleSidebarCollapseAsync();
            return;
        }

        this.ToggleSidebar();
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await this.StopDatabaseMonitorAsync();

        if (!this.disposed && this.hotkeyRegistered)
        {
            try
            {
                await this.JS.InvokeVoidAsync("sidebarManager.unregisterHotkey");
            }
            catch
            {
                // Ignore disposal errors during circuit shutdown.
            }
            finally
            {
                this.hotkeyRegistered = false;
            }
        }

        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;
        this.AuthService.AuthStateChanged += this.OnStateChanged;
        this.Navigation.LocationChanged += this.OnLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || this.authRestored)
        {
            return;
        }

        await this.Theme.InitializeAsync();
        await this.AuthService.EnsureRestoredAsync();

        this.isSidebarCollapsed = await this.JS.InvokeAsync<bool>("sidebarManager.getCollapsed");
        this.dotNetRef = DotNetObjectReference.Create(this);
        await this.JS.InvokeVoidAsync("sidebarManager.registerHotkey", this.dotNetRef!);
        this.hotkeyRegistered = true;

        this.authRestored = true;
        this.StartDatabaseMonitor();
        await this.LoadNotificationsAsync();
        await this.InvokeAsync(this.StateHasChanged);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
            this.L.LanguageChanged -= this.OnStateChanged;
            this.Theme.ThemeChanged -= this.OnStateChanged;
            this.AuthService.AuthStateChanged -= this.OnStateChanged;
            this.Navigation.LocationChanged -= this.OnLocationChanged;

            this.dotNetRef?.Dispose();
            this.dotNetRef = null;

            this.databaseMonitorCts?.Cancel();
            this.databaseMonitorCts?.Dispose();
            this.databaseMonitorCts = null;
        }

        this.disposed = true;
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        this.isNotificationsOpen = false;

        if (!this.isSidebarOpen)
        {
            return;
        }

        this.isSidebarOpen = false;
        this.InvokeAsync(this.StateHasChanged);
    }

    private void ToggleSidebar() => this.isSidebarOpen = !this.isSidebarOpen;

    private void CloseSidebar() => this.isSidebarOpen = false;

    private async Task ToggleSidebarCollapseAsync()
    {
        this.isSidebarCollapsed = !this.isSidebarCollapsed;
        await this.JS.InvokeVoidAsync("sidebarManager.setCollapsed", this.isSidebarCollapsed);
    }

    private async Task HandleLogout()
    {
        await this.AuthService.LogoutAsync();
        this.notifications.Clear();
        this.UnreadNotificationsCount = 0;
        this.isNotificationsOpen = false;
        this.Navigation.NavigateTo("/login");
    }

    private async Task ToggleNotificationsAsync()
    {
        this.isNotificationsOpen = !this.isNotificationsOpen;

        if (this.isNotificationsOpen)
        {
            await this.LoadNotificationsAsync();

            var userId = this.AuthService.CurrentUser?.Id;
            if (userId is not null && this.UnreadNotificationsCount > 0)
            {
                await this.NotificationService.MarkAllAsReadAsync(userId.Value);
                await this.LoadNotificationsAsync();
            }
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
    }

    private void StartDatabaseMonitor()
    {
        if (this.databaseMonitorCts is not null)
        {
            return;
        }

        this.databaseMonitorCts = new CancellationTokenSource();
        this.databaseMonitorTask = this.MonitorDatabaseConnectionAsync(this.databaseMonitorCts.Token);
    }

    private async Task StopDatabaseMonitorAsync()
    {
        if (this.databaseMonitorCts is null)
        {
            return;
        }

        await this.databaseMonitorCts.CancelAsync();

        if (this.databaseMonitorTask is not null)
        {
            try
            {
                await this.databaseMonitorTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown.
            }
        }

        this.databaseMonitorCts.Dispose();
        this.databaseMonitorCts = null;
        this.databaseMonitorTask = null;
    }

    private async Task MonitorDatabaseConnectionAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(DatabaseMonitorInterval);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var isAvailable = await this.IsDatabaseAvailableAsync(cancellationToken);

            if (!isAvailable && !this.databaseConnectionLost)
            {
                this.databaseConnectionLost = true;
                await this.ShowToastAsync(this.L.Translate("db.connectionLost"), ToastType.Error);
                continue;
            }

            if (isAvailable && this.databaseConnectionLost)
            {
                this.databaseConnectionLost = false;
                await this.ShowToastAsync(this.L.Translate("db.connectionRestored"), ToastType.Success);
            }
        }
    }

    private async Task<bool> IsDatabaseAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = this.HttpClientFactory.CreateClient();
            client.BaseAddress = new Uri(this.Navigation.BaseUri);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await client.GetFromJsonAsync<DatabaseHealthResponse>("health/db", timeoutCts.Token);
            return response?.IsAvailable ?? false;
        }
        catch
        {
            return false;
        }
    }

    private async Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 5000)
    {
        var toast = new ToastMessage { Message = message, Type = type };
        this.toasts.Add(toast);
        await this.InvokeAsync(this.StateHasChanged);

        _ = Task.Run(async () =>
        {
            await Task.Delay(durationMs);

            toast.IsHiding = true;
            await this.InvokeAsync(this.StateHasChanged);

            await Task.Delay(300);
            this.toasts.Remove(toast);
            await this.InvokeAsync(this.StateHasChanged);
        });
    }

    private async Task RemoveToastAsync(ToastMessage toast)
    {
        toast.IsHiding = true;
        await this.InvokeAsync(this.StateHasChanged);
        await Task.Delay(300);
        this.toasts.Remove(toast);
        await this.InvokeAsync(this.StateHasChanged);
    }

    private sealed class DatabaseHealthResponse
    {
        public bool IsAvailable { get; init; }
    }
}
