namespace ServiceDeskSystem.Application.Services.Preferences;

/// <summary>
/// Preference names and allowed values. Mirrors <c>SCHEMA</c> in <c>wwwroot/js/prefs.js</c>
/// (theme excluded: it is owned by <c>ThemeService</c>).
/// </summary>
public static class PreferenceSchema
{
    public const string Accent = "accent";
    public const string Scale = "scale";
    public const string Contrast = "contrast";
    public const string ReducedMotion = "reducedMotion";
    public const string Density = "density";
    public const string ContentWidth = "contentWidth";
    public const string DateFormat = "dateFormat";
    public const string DefaultView = "defaultView";
    public const string Sound = "sound";
    public const string DesktopNotifications = "desktopNotifications";
    public const string ToastPosition = "toastPosition";
    public const string Shortcuts = "shortcuts";

    private static readonly string[] Bool = ["true", "false"];

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Allowed { get; } =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [Accent] = ["lime", "amber", "phosphor", "ice"],
            [Scale] = ["90", "100", "110", "120"],
            [Contrast] = ["standard", "high"],
            [ReducedMotion] = Bool,
            [Density] = ["comfortable", "compact"],
            [ContentWidth] = ["narrow", "standard", "wide"],
            [DateFormat] = ["DD/MM/YYYY", "MM/DD/YYYY", "YYYY-MM-DD"],
            [DefaultView] = ["TABLE", "KANBAN"],
            [Sound] = Bool,
            [DesktopNotifications] = Bool,
            [ToastPosition] = ["top-right", "top-center", "bottom-right", "bottom-center"],
            [Shortcuts] = Bool,
        };

    public static bool IsAllowed(string name, string value) =>
        Allowed.TryGetValue(name, out var values) && values.Contains(value);
}
