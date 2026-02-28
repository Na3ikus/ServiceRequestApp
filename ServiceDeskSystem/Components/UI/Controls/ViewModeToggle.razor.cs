using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Controls;

/// <summary>
/// Toggle control for switching between Table and Kanban view modes.
/// </summary>
public partial class ViewModeToggle : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public string CurrentMode { get; set; } = "Table";

    [Parameter]
    public EventCallback<string> OnModeChanged { get; set; }
}
