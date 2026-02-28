using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Controls;

/// <summary>
/// Displays a statistics card with gradient icon, label, numeric value, and colored accent bar.
/// </summary>
public partial class StatCard : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public int Value { get; set; }

    [Parameter]
    public string GradientFrom { get; set; } = "blue-500";

    [Parameter]
    public string GradientTo { get; set; } = "blue-600";

    [Parameter]
    public string ShadowColor { get; set; } = "blue";

    [Parameter]
    [EditorRequired]
    public RenderFragment IconContent { get; set; } = null!;
}
