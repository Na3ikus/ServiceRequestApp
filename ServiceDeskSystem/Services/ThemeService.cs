namespace ServiceDeskSystem.Services;

internal sealed class ThemeService : IThemeService
{
    private string currentTheme = "light";

    public event EventHandler? ThemeChanged;

    public string CurrentTheme => this.currentTheme;

    public bool IsDarkMode => this.currentTheme == "dark";

    public void SetTheme(string theme)
    {
        if ((theme == "light" || theme == "dark") && this.currentTheme != theme)
        {
            this.currentTheme = theme;
            this.ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ToggleTheme()
    {
        this.currentTheme = this.currentTheme == "light" ? "dark" : "light";
        this.ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
