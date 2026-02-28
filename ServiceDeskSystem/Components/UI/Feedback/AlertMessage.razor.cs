using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Feedback;

/// <summary>
/// Displays an alert message with contextual type styling (error, success, warning, info).
/// </summary>
public partial class AlertMessage : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public AlertType Type { get; set; } = AlertType.Error;

    private string ContainerClass => this.Type switch
    {
        AlertType.Error => "mb-6 p-4 bg-red-500/10 border border-red-500/20 rounded-xl backdrop-blur-md shadow-[0_0_15px_rgba(239,68,68,0.1)]",
        AlertType.Success => "mb-6 p-4 bg-green-500/10 border border-green-500/20 rounded-xl backdrop-blur-md shadow-[0_0_15px_rgba(34,197,94,0.1)]",
        AlertType.Warning => "mb-6 p-4 bg-amber-500/10 border border-amber-500/20 rounded-xl backdrop-blur-md shadow-[0_0_15px_rgba(245,158,11,0.1)]",
        AlertType.Info => "mb-6 p-4 bg-blue-500/10 border border-blue-500/20 rounded-xl backdrop-blur-md shadow-[0_0_15px_rgba(59,130,246,0.1)]",
        _ => "mb-6 p-4 bg-red-500/10 border border-red-500/20 rounded-xl backdrop-blur-md",
    };

    private string TextClass => this.Type switch
    {
        AlertType.Error => "text-red-400",
        AlertType.Success => "text-green-400",
        AlertType.Warning => "text-amber-400",
        AlertType.Info => "text-blue-400",
        _ => "text-red-400",
    };
}
