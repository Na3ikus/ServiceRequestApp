namespace ServiceDeskSystem.Application.Services.Theme;

public interface IThemeService
{
    event EventHandler? ThemeChanged;

    string CurrentTheme { get; }

    bool IsDarkMode { get; }

    bool IsSystemTheme { get; }

    Task InitializeAsync();

    void SetTheme(string theme);

    void ToggleTheme();

    bool IsSidebarCollapsed { get; }

    Task SetSidebarCollapsedAsync(bool collapsed);
}

