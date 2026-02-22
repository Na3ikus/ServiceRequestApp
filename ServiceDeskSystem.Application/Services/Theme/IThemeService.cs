namespace ServiceDeskSystem.Application.Services.Theme;

public interface IThemeService
{
    event EventHandler? ThemeChanged;

    string CurrentTheme { get; }

    bool IsDarkMode { get; }

    Task InitializeAsync();

    void SetTheme(string theme);

    void ToggleTheme();
}
