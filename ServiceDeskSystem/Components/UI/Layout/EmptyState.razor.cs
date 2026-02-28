using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Layout;

/// <summary>
/// Displays an empty state placeholder with icon, title, description, and optional CTA link.
/// </summary>
public partial class EmptyState : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public string Description { get; set; } = string.Empty;

    [Parameter]
    public string? ActionText { get; set; }

    [Parameter]
    public string? ActionHref { get; set; }

    [Parameter]
    public RenderFragment? IconContent { get; set; }

    [Parameter]
    public RenderFragment? ActionIconContent { get; set; }
}
