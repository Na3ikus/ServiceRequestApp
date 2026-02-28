using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Layout;

/// <summary>
/// Renders a modal dialog overlay with title, body content, and optional footer.
/// </summary>
public partial class ModalDialog : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public bool IsVisible { get; set; }

    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public RenderFragment? FooterContent { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }
}
