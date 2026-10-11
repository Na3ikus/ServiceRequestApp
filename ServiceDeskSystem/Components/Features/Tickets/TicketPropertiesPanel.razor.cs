using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Features.Tickets;

/// <summary>
/// New start / due values picked in the properties panel, already converted to UTC.
/// </summary>
#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
public sealed record TicketDatesChange(DateTime? StartUtc, DateTime? DueUtc);
#pragma warning restore SA1313

/// <summary>
/// Properties of a ticket (status, priority, people, dates, tags). Admins and developers edit
/// priority, dates and tags here; unsaved edits survive the page's auto-refresh.
/// </summary>
public partial class TicketPropertiesPanel : BaseComponent
{
    private const string RowClass = "grid min-h-10 grid-cols-[6rem_minmax(0,1fr)] items-center gap-2.5 border-b border-dashed border-line px-3.5 py-2";
    private const string LabelClass = "text-13 text-muted";
    private const string ValueClass = "flex min-w-0 flex-wrap items-center gap-1.5 text-14 text-fg";

    private TicketPriority editPriority;
    private bool priorityDirty;
    private bool criticalConfirmed;
    private DateTime? startEdit;
    private DateTime? dueEdit;
    private bool startDirty;
    private bool dueDirty;
    private bool tagMenuOpen;

    [Parameter]
    [EditorRequired]
    public Ticket Ticket { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether priority, dates and tags can be edited (admin/developer).
    /// </summary>
    [Parameter]
    public bool CanEdit { get; set; }

    [Parameter]
    public IReadOnlyList<Tag> AllTags { get; set; } = [];

    [Parameter]
    public EventCallback<TicketPriority> OnSavePriority { get; set; }

    [Parameter]
    public EventCallback<TicketDatesChange> OnSaveDates { get; set; }

    [Parameter]
    public EventCallback<int> OnAddTag { get; set; }

    [Parameter]
    public EventCallback<int> OnRemoveTag { get; set; }

    private bool ShowPrioritySave => this.CanEdit && (this.editPriority != this.Ticket.Priority || !this.Ticket.IsPriorityAssessed);

    private bool DatesChanged => this.CanEdit &&
        ((this.startDirty && this.startEdit != TicketDates.ToInput(this.Ticket.StartDate, this.UserZone)) ||
         (this.dueDirty && this.dueEdit != TicketDates.ToInput(this.Ticket.DueDate, this.UserZone)));

    private List<Tag> AvailableTags => this.AllTags.Where(t => this.Ticket.Tags.All(assigned => assigned.Id != t.Id)).ToList();

    protected override void OnParametersSet()
    {
        ArgumentNullException.ThrowIfNull(this.Ticket);
        this.SyncFromTicket();
    }

    protected override void OnTimeZoneChanged(object? sender, EventArgs e)
    {
        _ = this.InvokeAsync(() =>
        {
            this.SyncFromTicket();
            this.StateHasChanged();
        });
    }

    private void SyncFromTicket()
    {
        // Values the user has not touched follow the ticket; edits in progress are kept.
        if (!this.priorityDirty)
        {
            this.editPriority = this.Ticket.Priority;
        }

        if (!this.startDirty)
        {
            this.startEdit = TicketDates.ToInput(this.Ticket.StartDate, this.UserZone);
        }

        if (!this.dueDirty)
        {
            this.dueEdit = TicketDates.ToInput(this.Ticket.DueDate, this.UserZone);
        }
    }

    private string Format(DateTime utc) => this.ToLocal(utc).ToString("dd MMM yyyy, HH:mm", this.UiCulture);

    private string RemoveTagLabel(Tag tag) => $"{this.L.Translate("common.delete")}: {tag.Name}";

    private void MarkStartDirty() => this.startDirty = true;

    private void MarkDueDirty() => this.dueDirty = true;

    private void OnPriorityChanged()
    {
        this.priorityDirty = this.editPriority != this.Ticket.Priority;
        this.criticalConfirmed = false;
    }

    private async Task SavePriorityAsync()
    {
        if (this.editPriority == TicketPriority.Critical && !this.criticalConfirmed)
        {
            return;
        }

        await this.OnSavePriority.InvokeAsync(this.editPriority);
        this.priorityDirty = false;
        this.criticalConfirmed = false;
        this.SyncFromTicket();
    }

    private async Task SaveDatesAsync()
    {
        // An untouched field keeps its stored value exactly (the input drops seconds).
        var start = this.startDirty ? TicketDates.ToStorage(this.startEdit, this.UserZone) : this.Ticket.StartDate;
        var due = this.dueDirty ? TicketDates.ToStorage(this.dueEdit, this.UserZone) : this.Ticket.DueDate;

        await this.OnSaveDates.InvokeAsync(new TicketDatesChange(start, due));
        this.startDirty = false;
        this.dueDirty = false;
        this.SyncFromTicket();
    }

    private void ToggleTagMenu() => this.tagMenuOpen = !this.tagMenuOpen;

    private void CloseTagMenu() => this.tagMenuOpen = false;

    private async Task AddTagAsync(int tagId)
    {
        this.tagMenuOpen = false;
        await this.OnAddTag.InvokeAsync(tagId);
    }

    private void OnTagMenuKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            this.tagMenuOpen = false;
        }
    }
}
