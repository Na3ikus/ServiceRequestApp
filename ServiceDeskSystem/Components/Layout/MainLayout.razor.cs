using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications.Models;
using ServiceDeskSystem.Application.Services.Theme.Interfaces;
using ServiceDeskSystem.Application.Services.Toasts.Interfaces;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;

namespace ServiceDeskSystem.Components.Layout;

/// <summary>
/// Main application layout with responsive sidebar and header actions.
/// </summary>
public partial class MainLayout : LayoutComponentBase, IDisposable, IAsyncDisposable
{
    private static readonly TimeSpan DatabaseMonitorInterval = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan NotificationPulseDuration = TimeSpan.FromSeconds(2);
    private readonly List<UserNotificationDto> notifications = [];

    private bool authRestored;
    private bool isLanguageDropdownOpen;
    private bool isNotificationsOpen;
    private bool isSidebarOpen;
    private bool isSidebarCollapsed;
    private bool hotkeyRegistered;
    private bool databaseConnectionLost;
    private bool hasNewNotificationPulse;
    private bool notificationsHubInitialized;
    private bool disposed;
    private int lastKnownUnreadCount;
    private DotNetObjectReference<MainLayout>? dotNetRef;
    private CancellationTokenSource? databaseMonitorCts;
    private Task? databaseMonitorTask;
    private HubConnection? notificationsHubConnection;
    private CancellationTokenSource? notificationPulseCts;

    internal IReadOnlyList<UserNotificationDto> Notifications => this.notifications;
    internal int UnreadNotificationsCount { get; private set; }
    internal bool IsNotificationsOpen => this.isNotificationsOpen;
    internal bool HasNewNotificationPulse => this.hasNewNotificationPulse;

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

    [Inject]
    private IToastService ToastService { get; set; } = null!;

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

    [JSInvokable]
    public Task HandleThemeHotkey()
    {
        this.Theme.ToggleTheme();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task HandleOutsideLanguageClick()
    {
        if (!this.isLanguageDropdownOpen)
        {
            return;
        }

        this.isLanguageDropdownOpen = false;
        await this.UnregisterLanguageOutsideClickAsync();
        await this.InvokeAsync(this.StateHasChanged);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await this.StopDatabaseMonitorAsync();
        await this.StopNotificationsHubAsync();
        await this.UnregisterLanguageOutsideClickAsync();
        await this.UnregisterNotificationOutsideClickAsync();

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
        this.ToastService.OnToastsChanged += this.OnStateChanged;
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
        await this.StartNotificationsHubAsync();
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
            this.ToastService.OnToastsChanged -= this.OnStateChanged;

            this.dotNetRef?.Dispose();
            this.dotNetRef = null;

            this.databaseMonitorCts?.Cancel();
            this.databaseMonitorCts?.Dispose();
            this.databaseMonitorCts = null;

            this.notificationsHubConnection = null;
            this.notificationsHubInitialized = false;

            this.notificationPulseCts?.Cancel();
            this.notificationPulseCts?.Dispose();
            this.notificationPulseCts = null;
        }

        this.disposed = true;
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = this.UnregisterLanguageOutsideClickAsync();
        _ = this.UnregisterNotificationOutsideClickAsync();
        this.isLanguageDropdownOpen = false;
        this.StopNotificationPulse();
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
        await this.UnregisterLanguageOutsideClickAsync();
        await this.UnregisterNotificationOutsideClickAsync();
        this.isLanguageDropdownOpen = false;
        this.notifications.Clear();
        this.UnreadNotificationsCount = 0;
        this.lastKnownUnreadCount = 0;
        this.StopNotificationPulse();
        this.isNotificationsOpen = false;
        this.Navigation.NavigateTo("/login");
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
                await this.ToastService.ShowToastAsync(this.L.Translate("db.connectionLost"), ToastType.Error);
                continue;
            }

            if (isAvailable && this.databaseConnectionLost)
            {
                this.databaseConnectionLost = false;
                await this.ToastService.ShowToastAsync(this.L.Translate("db.connectionRestored"), ToastType.Success);
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

    private string GetHeaderDateText()
    {
        var culture = this.L.CurrentLanguage == "uk"
            ? CultureInfo.GetCultureInfo("uk-UA")
            : CultureInfo.GetCultureInfo("en-US");

        return DateTime.Now.ToString("dddd, MMMM dd, yyyy", culture);
    }

    private string FormatNotificationTime(DateTime utcDateTime)
    {
        var culture = this.L.CurrentLanguage == "uk"
            ? CultureInfo.GetCultureInfo("uk-UA")
            : CultureInfo.GetCultureInfo("en-US");

        return utcDateTime.ToLocalTime().ToString("g", culture);
    }

    private async Task ToggleLanguageDropdownAsync()
    {
        this.isLanguageDropdownOpen = !this.isLanguageDropdownOpen;

        if (this.isLanguageDropdownOpen)
        {
            await this.RegisterLanguageOutsideClickAsync();
        }
        else
        {
            await this.UnregisterLanguageOutsideClickAsync();
        }
    }

    private async Task SetLanguageAsync(string language)
    {
        this.L.SetLanguage(language);
        this.isLanguageDropdownOpen = false;
        await this.UnregisterLanguageOutsideClickAsync();
    }

    private async Task RegisterLanguageOutsideClickAsync()
    {
        if (this.dotNetRef is null)
        {
            return;
        }

        try
        {
            await this.JS.InvokeVoidAsync("notificationManager.registerLanguageOutsideClick", this.dotNetRef);
        }
        catch
        {
            // Ignore JS interop issues during reconnect/shutdown.
        }
    }

    private async Task UnregisterLanguageOutsideClickAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("notificationManager.unregisterLanguageOutsideClick");
        }
        catch
        {
            // Ignore JS interop issues during reconnect/shutdown.
        }
    }

    private sealed class DatabaseHealthResponse
    {
        public bool IsAvailable { get; init; }
    }
}
