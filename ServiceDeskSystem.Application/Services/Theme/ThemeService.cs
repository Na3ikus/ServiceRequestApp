using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Theme;

namespace ServiceDeskSystem.Application.Services.Theme;

public sealed class ThemeService : IThemeService
{
    private readonly IJSRuntime jsRuntime;
    private string currentTheme = "light";
    private string preferredTheme = "light"; // "light" | "dark" | "system"
    private bool initialized;
    private bool isSidebarCollapsed;

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
                this.preferredTheme = await this.jsRuntime.InvokeAsync<string>("themeManager.getTheme");

                if (this.preferredTheme == "system")
                {
                    var prefersDark = await this.jsRuntime.InvokeAsync<bool>(
                        "eval", "window.matchMedia('(prefers-color-scheme: dark)').matches");
                    this.currentTheme = prefersDark ? "dark" : "light";
                    var action = this.currentTheme == "dark" ? "add" : "remove";
                    await this.jsRuntime.InvokeVoidAsync(
                        "eval",
                        $"document.documentElement.classList.{action}('dark')");
                }
                else
                {
                    this.currentTheme = this.preferredTheme;
                }

                this.isSidebarCollapsed = await this.jsRuntime.InvokeAsync<bool>("sidebarManager.getCollapsed");
                this.initialized = true;
            }
            catch
            {
                this.currentTheme = "light";
                this.preferredTheme = "light";
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

        string resolvedTheme;
        if (theme == "system")
        {
            try
            {
                var prefersDark = await this.jsRuntime.InvokeAsync<bool>(
                    "eval", "window.matchMedia('(prefers-color-scheme: dark)').matches");
                resolvedTheme = prefersDark ? "dark" : "light";
            }
            catch
            {
                resolvedTheme = "light";
            }
        }
        else
        {
            resolvedTheme = theme;
        }

        if (this.currentTheme == resolvedTheme && this.preferredTheme != "system")
        {
            return;
        }

        this.currentTheme = resolvedTheme;

        try
        {
            // Save preference ("system", "light", or "dark")
            await this.jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", this.preferredTheme);
            // Apply resolved class to <html>
            var action = resolvedTheme == "dark" ? "add" : "remove";
            await this.jsRuntime.InvokeVoidAsync(
                "eval",
                $"document.documentElement.classList.{action}('dark')");
        }
        catch
        {
            // Ignore JS interop errors during prerendering
        }

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
}
