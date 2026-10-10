using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ServiceDeskSystem.Components.Pages.Settings.Controls;

/// <summary>
/// Single-choice control rendered as an ARIA radio group with roving tabindex and
/// arrow-key navigation.
/// </summary>
public partial class SegmentedControl : ComponentBase
{
    private ElementReference[] buttons = [];

    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<(string Value, string Text)> Options { get; set; } = [];

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    private int SelectedIndex
    {
        get
        {
            for (var i = 0; i < this.Options.Count; i++)
            {
                if (this.Options[i].Value == this.Value)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    protected override void OnParametersSet()
    {
        if (this.buttons.Length != this.Options.Count)
        {
            this.buttons = new ElementReference[this.Options.Count];
        }
    }

    private static string ButtonClass(bool selected, int index)
    {
        var border = index > 0 ? " border-l border-line" : string.Empty;
        var state = selected ? " bg-fg text-canvas" : " text-muted hover:text-fg";
        return "px-3 py-1.5 transition-colors focus-visible:outline focus-visible:outline-1 focus-visible:outline-accent-ink focus-visible:-outline-offset-2" + border + state;
    }

    // Exactly one option is reachable with Tab: the selected one, or the first if none is.
    private bool IsTabStop(int index) => index == Math.Max(this.SelectedIndex, 0);

    private async Task SelectAsync(int index)
    {
        var value = this.Options[index].Value;
        if (value != this.Value)
        {
            await this.ValueChanged.InvokeAsync(value);
        }
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs e, int index)
    {
        var count = this.Options.Count;
        int? target = e.Key switch
        {
            "ArrowRight" or "ArrowDown" => (index + 1) % count,
            "ArrowLeft" or "ArrowUp" => (index - 1 + count) % count,
            "Home" => 0,
            "End" => count - 1,
            _ => null,
        };

        if (target is not int next)
        {
            return;
        }

        await this.SelectAsync(next);

        try
        {
            await this.buttons[next].FocusAsync();
        }
        catch
        {
            // Element may not be rendered yet (e.g. prerendering).
        }
    }
}
