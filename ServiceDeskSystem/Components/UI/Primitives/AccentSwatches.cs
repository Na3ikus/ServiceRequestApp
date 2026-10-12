namespace ServiceDeskSystem.Components.UI.Primitives;

/// <summary>
/// Swatch colors of the accent presets (<c>html[data-accent]</c> in <c>Styles/input.css</c>).
/// Fixed hex values, so a swatch shows its own accent whatever accent is active.
/// </summary>
public static class AccentSwatches
{
    // Dark-theme accent values so they read the same in both themes.
    public static IReadOnlyList<(string Name, string Hex)> All { get; } =
    [
        ("lime", "#c6f432"),
        ("amber", "#ffb000"),
        ("phosphor", "#33ff99"),
        ("ice", "#4de8f0"),
    ];
}
