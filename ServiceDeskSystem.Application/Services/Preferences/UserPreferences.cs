namespace ServiceDeskSystem.Application.Services.Preferences;

public sealed record UserPreferences(
    string Accent,
    int Scale,
    bool HighContrast,
    bool ReducedMotion,
    string Density,
    string ContentWidth,
    string DateFormat,
    string DefaultView,
    bool Sound,
    bool DesktopNotifications,
    string ToastPosition,
    bool Shortcuts)
{
    public static UserPreferences Defaults { get; } = new(
        Accent: "lime",
        Scale: 100,
        HighContrast: false,
        ReducedMotion: false,
        Density: "comfortable",
        ContentWidth: "standard",
        DateFormat: "DD/MM/YYYY",
        DefaultView: "TABLE",
        Sound: true,
        DesktopNotifications: false,
        ToastPosition: "top-right",
        Shortcuts: true);

    /// <summary>Returns a copy with one preference changed; <paramref name="value"/> must be allowed.</summary>
    public UserPreferences With(string name, string value) => name switch
    {
        PreferenceSchema.Accent => this with { Accent = value },
        PreferenceSchema.Scale => this with { Scale = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture) },
        PreferenceSchema.Contrast => this with { HighContrast = value == "high" },
        PreferenceSchema.ReducedMotion => this with { ReducedMotion = value == "true" },
        PreferenceSchema.Density => this with { Density = value },
        PreferenceSchema.ContentWidth => this with { ContentWidth = value },
        PreferenceSchema.DateFormat => this with { DateFormat = value },
        PreferenceSchema.DefaultView => this with { DefaultView = value },
        PreferenceSchema.Sound => this with { Sound = value == "true" },
        PreferenceSchema.DesktopNotifications => this with { DesktopNotifications = value == "true" },
        PreferenceSchema.ToastPosition => this with { ToastPosition = value },
        PreferenceSchema.Shortcuts => this with { Shortcuts = value == "true" },
        _ => throw new ArgumentException($"Unknown preference '{name}'.", nameof(name)),
    };
}
