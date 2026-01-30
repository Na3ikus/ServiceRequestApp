namespace ServiceDeskSystem.Services;

internal interface IThemeService
{
    event EventHandler? ThemeChanged;

    string CurrentTheme { get; }

    bool IsDarkMode { get; }

    void SetTheme(string theme);

    void ToggleTheme();
}
