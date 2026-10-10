namespace ServiceDeskSystem.Components.UI.Primitives;

/// <summary>
/// One crumb in a <see cref="PageHeader"/> path, optionally linking elsewhere.
/// </summary>
#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
public sealed record PathSegment(string Text, string? Href = null);
#pragma warning restore SA1313
