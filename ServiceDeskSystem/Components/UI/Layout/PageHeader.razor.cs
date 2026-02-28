using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Layout;

/// <summary>
/// Displays a page header with title and optional subtitle.
/// </summary>
public partial class PageHeader : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Subtitle { get; set; }
}
