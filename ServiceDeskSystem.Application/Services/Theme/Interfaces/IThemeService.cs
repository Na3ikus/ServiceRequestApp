namespace ServiceDeskSystem.Application.Services.Theme.Interfaces;

public interface IThemeService
{
    event EventHandler? ThemeChanged;

    string CurrentTheme { get; }

    bool IsDarkMode { get; }

    Task InitializeAsync();

    void SetTheme(string theme);

    void ToggleTheme();
}
