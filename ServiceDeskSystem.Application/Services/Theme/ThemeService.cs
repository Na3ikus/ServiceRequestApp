using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Theme;

namespace ServiceDeskSystem.Application.Services.Theme;

public sealed class ThemeService : IThemeService
{
    private readonly IJSRuntime jsRuntime;
    private string currentTheme = "light";
    private bool initialized;
    private bool isSidebarCollapsed;

    public ThemeService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public event EventHandler? ThemeChanged;

    public string CurrentTheme => this.currentTheme;

    public bool IsDarkMode => this.currentTheme == "dark";

    public bool IsSidebarCollapsed => this.isSidebarCollapsed;

    public async Task InitializeAsync()
    {
        if (!this.initialized)
        {
            try
            {
                this.currentTheme = await this.jsRuntime.InvokeAsync<string>("themeManager.getTheme");
                this.isSidebarCollapsed = await this.jsRuntime.InvokeAsync<bool>("sidebarManager.getCollapsed");
                this.initialized = true;
            }
            catch
            {
                this.currentTheme = "light";
            }
        }
    }

    public async void SetTheme(string theme)
    {
        if ((theme == "light" || theme == "dark") && this.currentTheme != theme)
        {
            this.currentTheme = theme;

            try
            {
                await this.jsRuntime.InvokeVoidAsync("themeManager.setTheme", theme);
            }
            catch
            {
                // Ignore JS interop errors during prerendering
            }

            this.ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ToggleTheme()
    {
        var newTheme = this.currentTheme == "light" ? "dark" : "light";
        this.SetTheme(newTheme);
    }

    public async Task SetSidebarCollapsedAsync(bool collapsed)
    {
        if (this.isSidebarCollapsed != collapsed)
        {
            this.isSidebarCollapsed = collapsed;

            try
            {
                await this.jsRuntime.InvokeVoidAsync("sidebarManager.setCollapsed", collapsed);
            }
            catch
            {
                // Ignore JS interop errors during prerendering
            }

            this.ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

