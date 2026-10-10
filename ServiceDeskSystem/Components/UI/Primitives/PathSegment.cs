namespace ServiceDeskSystem.Components.UI.Primitives;

/// <summary>
/// One crumb in a <see cref="PageHeader"/> path, optionally linking elsewhere.
/// </summary>
public sealed record PathSegment(string Text, string? Href = null);
