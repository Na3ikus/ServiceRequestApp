using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Components.UI.Controls;

/// <summary>
/// Primary gradient button with built-in loading spinner state.
/// </summary>
public partial class GradientButton : ComponentBase
{
    [Parameter]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    public string? LoadingText { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public string Type { get; set; } = "button";

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    private string ButtonClass => string.IsNullOrEmpty(this.CssClass)
        ? "px-6 py-3 bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white font-medium rounded-xl transition-all shadow-lg shadow-blue-500/25 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        : this.CssClass;
}
