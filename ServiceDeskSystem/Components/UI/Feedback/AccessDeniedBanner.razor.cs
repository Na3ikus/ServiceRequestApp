using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Feedback;

/// <summary>
/// Displays an access denied warning banner with an icon and message.
/// </summary>
public partial class AccessDeniedBanner : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Message { get; set; } = string.Empty;
}
