using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;
using ServiceDeskSystem.Application.Services.Theme.Interfaces;

namespace ServiceDeskSystem.Components.Layout;

/// <summary>
/// Main application layout with responsive sidebar and header actions.
/// </summary>
public partial class MainLayout : LayoutComponentBase, IDisposable, IAsyncDisposable
{
    private bool authRestored;
    private bool isSidebarOpen;
    private bool isSidebarCollapsed;
    private bool hotkeyRegistered;
    private bool disposed;
    private DotNetObjectReference<MainLayout>? dotNetRef;

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
        }

        this.disposed = true;
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
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
        this.Navigation.NavigateTo("/login");
    }
}
