using Microsoft.JSInterop;

namespace ServiceDeskSystem.Application.Services.Theme;

/// <summary>
/// Theme state for Blazor components. Persistence and applying the <c>dark</c> class
/// live in <c>wwwroot/js/prefs.js</c>; this service mirrors them and follows OS changes.
/// </summary>
public sealed class ThemeService : IThemeService, IDisposable
{
    private readonly IJSRuntime jsRuntime;
    private string currentTheme = "light";
    private string preferredTheme = "system"; // "light" | "dark" | "system"
    private bool initialized;
    private bool isSidebarCollapsed;
    private DotNetObjectReference<ThemeService>? selfReference;

    public ThemeService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public event EventHandler? ThemeChanged;

    public string CurrentTheme => this.currentTheme;

    public bool IsDarkMode => this.currentTheme == "dark";

    public bool IsSystemTheme => this.preferredTheme == "system";

    public bool IsSidebarCollapsed => this.isSidebarCollapsed;

    public async Task InitializeAsync()
    {
        if (!this.initialized)
        {
            try
            {
                this.preferredTheme = await this.jsRuntime.InvokeAsync<string>("prefs.get", "theme");
                this.currentTheme = await this.jsRuntime.InvokeAsync<string>("prefs.resolveTheme");

                this.selfReference ??= DotNetObjectReference.Create(this);
                await this.jsRuntime.InvokeVoidAsync("prefs.onSystemThemeChange", this.selfReference);

                this.isSidebarCollapsed = await this.jsRuntime.InvokeAsync<bool>("sidebarManager.getCollapsed");
                this.initialized = true;
            }
            catch
            {
                this.currentTheme = "light";
                this.preferredTheme = "system";
            }
        }
    }

    public async void SetTheme(string theme)
    {
        if (theme != "light" && theme != "dark" && theme != "system")
        {
            return;
        }

        this.preferredTheme = theme;

        string resolvedTheme = theme;
        try
        {
            await this.jsRuntime.InvokeVoidAsync("prefs.set", "theme", theme);
            if (theme == "system")
            {
                resolvedTheme = await this.jsRuntime.InvokeAsync<string>("prefs.resolveTheme");
            }
        }
        catch
        {
            // Ignore JS interop errors during prerendering
            if (theme == "system")
            {
                resolvedTheme = "light";
            }
        }

        this.currentTheme = resolvedTheme;
        this.ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Called by <c>prefs.js</c> when the OS color scheme changes while the theme is <c>system</c>.</summary>
    [JSInvokable]
    public void OnSystemThemeChanged(bool isDark)
    {
        if (this.preferredTheme != "system")
        {
            return;
        }

        this.currentTheme = isDark ? "dark" : "light";
        this.ThemeChanged?.Invoke(this, EventArgs.Empty);
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

    public void Dispose()
    {
        this.selfReference?.Dispose();
        this.selfReference = null;
    }
}
