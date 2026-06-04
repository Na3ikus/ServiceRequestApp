using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Theme;

namespace ServiceDeskSystem.Application.Services.Theme;

public sealed class ThemeService : IThemeService
{
    private readonly IJSRuntime jsRuntime;
    private string currentTheme = "light";
    private bool initialized;

    public ThemeService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public event EventHandler? ThemeChanged;

    public string CurrentTheme => this.currentTheme;

    public bool IsDarkMode => this.currentTheme == "dark";

    public async Task InitializeAsync()
    {
        if (!this.initialized)
        {
            try
            {
                this.currentTheme = await this.jsRuntime.InvokeAsync<string>("themeManager.getTheme");
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
        SetTheme(newTheme);
    }
}

