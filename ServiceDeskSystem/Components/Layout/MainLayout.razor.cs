using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Components.Layout;

/// <summary>
/// Main application layout with responsive sidebar and header actions.
/// </summary>
public partial class MainLayout : LayoutComponentBase, IDisposable, IAsyncDisposable
{
    private static readonly TimeSpan DatabaseMonitorInterval = TimeSpan.FromSeconds(15);
    private readonly List<UserNotificationDto> notifications = [];

    private bool authRestored;
    private bool isNotificationsOpen;
    private bool isSidebarOpen;
    private bool hotkeyRegistered;
    private bool databaseConnectionLost;
    private bool disposed;
    private int lastKnownUnreadCount;
    private DotNetObjectReference<MainLayout>? dotNetRef;
    private CancellationTokenSource? databaseMonitorCts;
    private Task? databaseMonitorTask;
    private ServiceDeskSystem.Components.Shared.HotkeysModal? hotkeysModal;

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

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private IDbContextFactory<BugTrackerDbContext> DbContextFactory { get; set; } = null!;

    [Inject]
    private IRealtimeEvents RealtimeEvents { get; set; } = null!;

    [Inject]
    private ServiceDeskSystem.Application.Services.Time.IUserTimeZone UserZone { get; set; } = null!;

    /// <summary>
    /// Gets the signed-in user's full name (avatar initials, top bar), or the login when no name is set.
    /// </summary>
    private string? DisplayName
    {
        get
        {
            var user = this.AuthService.CurrentUser;
            var fullName = $"{user?.Person?.FirstName} {user?.Person?.LastName}".Trim();
            return fullName.Length > 0 ? fullName : user?.Login;
        }
    }

    [JSInvokable]
    public Task HandleThemeHotkey()
    {
        this.Theme.ToggleTheme();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task HandleHotkeysHelp()
    {
        this.hotkeysModal?.Show();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public void HandleNavigateHotkey(string targetRoute)
    {
        this.Navigation.NavigateTo(targetRoute);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await this.StopDatabaseMonitorAsync();
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
        this.RealtimeEvents.NotificationsChanged += this.OnNotificationsChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || this.authRestored)
        {
            return;
        }

        await this.Theme.InitializeAsync();
        await this.ReadBrowserTimeZoneAsync();
        await this.AuthService.EnsureRestoredAsync();

        this.dotNetRef = DotNetObjectReference.Create(this);
        await this.JS.InvokeVoidAsync("sidebarManager.registerHotkey", this.dotNetRef!);
        await this.JS.InvokeVoidAsync("eval", "window.showHotkeysModal = function() { document.dispatchEvent(new CustomEvent('show-hotkeys-help')); };");
        this.hotkeyRegistered = true;

        this.authRestored = true;
        this.StartDatabaseMonitor();
        await this.LoadNotificationsAsync();
        await this.RestoreVisualSettingsAsync();

        // Persist the current user role to localStorage so that JS hotkeys (e.g. T) can
        // route to the correct page without a Blazor round-trip.
        if (this.AuthService.CurrentUser is not null)
        {
            try
            {
                await this.JS.InvokeVoidAsync(
                    "localStorage.setItem",
                    "user.role",
                    this.AuthService.CurrentUser.Role.ToString().ToUpperInvariant());
            }
            catch
            {
                // Ignore JS interop errors during prerendering.
            }
        }

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
            this.RealtimeEvents.NotificationsChanged -= this.OnNotificationsChanged;

            this.dotNetRef?.Dispose();
            this.dotNetRef = null;

            this.databaseMonitorCts?.Cancel();
            this.databaseMonitorCts?.Dispose();
            this.databaseMonitorCts = null;
        }

        this.disposed = true;
    }

    private async Task RestoreVisualSettingsAsync()
    {
        try
        {
            var savedLang = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.language");
            if (!string.IsNullOrWhiteSpace(savedLang))
            {
                this.L.SetLanguage(savedLang);
            }

            await this.JS.InvokeVoidAsync("prefs.applyAll");
        }
        catch
        {
            // Ignore JS interop errors during prerendering
        }
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = this.UnregisterNotificationOutsideClickAsync();
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

    private async Task HandleLogout()
    {
        await this.AuthService.LogoutAsync();
        await this.UnregisterNotificationOutsideClickAsync();
        this.notifications.Clear();
        this.UnreadNotificationsCount = 0;
        this.lastKnownUnreadCount = 0;
        this.isNotificationsOpen = false;

        // Remove cached role from localStorage on logout.
        try
        {
            await this.JS.InvokeVoidAsync("localStorage.removeItem", "user.role");
        }
        catch
        {
            // Ignore JS interop errors.
        }

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
            await using var db = await this.DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            return await db.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    private CultureInfo GetCurrentCultureInfo()
    {
        var lang = this.L.SupportedLanguages.FirstOrDefault(l => string.Equals(l.Code, this.L.CurrentLanguage, StringComparison.OrdinalIgnoreCase));
        var cultureName = lang?.CultureName ?? this.L.CurrentLanguage;
        try
        {
            return CultureInfo.GetCultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    private async Task ReadBrowserTimeZoneAsync()
    {
        try
        {
            this.UserZone.Set(await this.JS.InvokeAsync<string?>("prefs.timeZone"));
        }
        catch (JSException)
        {
            // Older cached prefs.js: keep the server zone.
        }
    }

    private string FormatNotificationTime(DateTime utcDateTime)
    {
        return this.UserZone.ToLocal(utcDateTime).ToString("g", this.GetCurrentCultureInfo());
    }

    private sealed class DatabaseHealthResponse
    {
        public bool IsAvailable { get; set; }
    }
}
