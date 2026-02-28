using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Feedback;

/// <summary>
/// Displays an animated loading spinner with an optional text label.
/// </summary>
public partial class LoadingSpinner : ComponentBase
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string Size { get; set; } = "h-6 w-6";
}
