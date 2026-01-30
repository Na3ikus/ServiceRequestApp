namespace ServiceDeskSystem.Services;

internal interface IThemeService
{
    string CurrentTheme { get; }

    bool IsDarkMode { get; }

    void SetTheme(string theme);

    void ToggleTheme();

    event EventHandler? ThemeChanged;
}
