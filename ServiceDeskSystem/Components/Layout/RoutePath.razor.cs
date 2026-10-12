using Microsoft.AspNetCore.Components.Routing;
using ServiceDeskSystem.Components.UI.Primitives;

namespace ServiceDeskSystem.Components.Layout;

/// <summary>
/// Shows the current route as a terminal-style path (<c>~/tickets/42</c>) in the top bar.
/// </summary>
public partial class RoutePath : ServiceDeskSystem.Components.UI.Base.BaseComponent
{
    private IReadOnlyList<PathSegment> segments = [];

    /// <summary>
    /// Splits <paramref name="absoluteUri"/> into path segments relative to <paramref name="baseUri"/>.
    /// Query string and fragment are dropped; segments are URL-decoded and lowercased. Segments carry no
    /// links: the only nested route (<c>/ticket/{id}</c>) has no page at its parent path. The root
    /// <c>~</c> is not included.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "NavigationManager exposes Uri and BaseUri as strings")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Lowercase is the displayed path style, not a comparison")]
    public static IReadOnlyList<PathSegment> Segments(string absoluteUri, string baseUri)
    {
        ArgumentNullException.ThrowIfNull(absoluteUri);
        ArgumentNullException.ThrowIfNull(baseUri);

        var relative = absoluteUri.StartsWith(baseUri, StringComparison.OrdinalIgnoreCase)
            ? absoluteUri[baseUri.Length..]
            : new Uri(absoluteUri).AbsolutePath;

        var end = relative.IndexOfAny(['?', '#']);
        if (end >= 0)
        {
            relative = relative[..end];
        }

        return relative
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => new PathSegment(Uri.UnescapeDataString(p).ToLowerInvariant()))
            .ToList();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.segments = Segments(this.Navigation.Uri, this.Navigation.BaseUri);
        this.Navigation.LocationChanged += this.OnLocationChanged;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !this.disposed)
        {
            this.Navigation.LocationChanged -= this.OnLocationChanged;
        }

        base.Dispose(disposing);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        this.segments = Segments(e.Location, this.Navigation.BaseUri);
        this.InvokeAsync(this.StateHasChanged);
    }
}
