using System.Globalization;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin panel tags management tab component.
/// </summary>
public partial class TagsTab : BaseComponent
{
    private static readonly string[] PresetColors =
    [
        "#EF4444",
        "#F97316",
        "#F59E0B",
        "#10B981",
        "#06B6D4",
        "#3B82F6",
        "#6366F1",
        "#8B5CF6",
        "#EC4899",
        "#64748B",
    ];

    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public List<Tag>? Tags { get; set; }

    [Parameter]
    public EventCallback OnTagsChanged { get; set; }

    [Inject]
    protected ITagService TagService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected Tag EditingTag { get; set; } = new ();

    protected bool ShowModal { get; set; }

    protected string ModalTitle { get; set; } = string.Empty;

    protected bool IsEditing { get; set; }

    protected bool IsSaving { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string SearchQuery { get; set; } = string.Empty;

    protected string SortColumn { get; set; } = "name";

    protected bool SortAscending { get; set; } = true;

    protected List<Tag>? GetFilteredTags()
    {
        IComparer<string> comparer = this.SortAscending ? StringComparer.OrdinalIgnoreCase : new ReverseStringComparer();
        return this.Tags?
            .Where(t =>
                string.IsNullOrWhiteSpace(this.SearchQuery) ||
                t.Name.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(
                t => this.SortColumn switch
                {
                    "count" => (t.Tickets?.Count ?? 0).ToString("D10", CultureInfo.InvariantCulture),
                    _ => t.Name,
                },
                comparer)
            .ToList();
    }

    protected void SetSort(string column)
    {
        if (this.SortColumn == column)
        {
            this.SortAscending = !this.SortAscending;
        }
        else
        {
            this.SortColumn = column;
            this.SortAscending = true;
        }
    }

    protected string GetSortArrow(string column)
    {
        if (this.SortColumn != column)
        {
            return "↕";
        }

        return this.SortAscending ? "↑" : "↓";
    }

    protected void ShowAddTag()
    {
        this.EditingTag = new Tag { Color = "#3B82F6" };
        this.OpenModal(this.L.Translate("tags.addTag"), isEdit: false);
    }

    protected void EditTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        this.EditingTag = new Tag
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
        };
        this.OpenModal(this.L.Translate("tags.editTag"), isEdit: true);
    }

    protected async Task DeleteTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        try
        {
            var success = await this.TagService.DeleteTagAsync(tag.Id, this.AuthService.CurrentUser?.Id);
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("tags.deleted"), ToastType.Success);
                await this.OnTagsChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            await this.ToastService.ShowToastAsync($"Error deleting tag: {ex.Message}", ToastType.Error);
        }
    }

    protected void CloseModal()
    {
        this.ShowModal = false;
        this.ErrorMessage = null;
    }

    protected async Task SaveModal()
    {
        this.IsSaving = true;
        this.ErrorMessage = null;

        try
        {
            await this.SaveTagAsync();

            if (this.ErrorMessage is null)
            {
                await this.OnTagsChanged.InvokeAsync();
                this.CloseModal();
            }
        }
        catch (Exception ex)
        {
            this.ErrorMessage = ex.Message;
        }
        finally
        {
            this.IsSaving = false;
        }
    }

    private async Task SaveTagAsync()
    {
        if (string.IsNullOrWhiteSpace(this.EditingTag.Name))
        {
            this.ErrorMessage = this.L.Translate("tags.nameRequired");
            return;
        }

        if (this.IsEditing)
        {
            await this.TagService.UpdateTagAsync(
                this.EditingTag.Id,
                this.EditingTag.Name,
                this.EditingTag.Color,
                this.AuthService.CurrentUser?.Id);
            await this.ToastService.ShowToastAsync(this.L.Translate("tags.updated"), ToastType.Success);
        }
        else
        {
            await this.TagService.CreateTagAsync(
                this.EditingTag.Name,
                this.EditingTag.Color,
                this.AuthService.CurrentUser?.Id);
            await this.ToastService.ShowToastAsync(this.L.Translate("tags.created"), ToastType.Success);
        }
    }

    private void OpenModal(string title, bool isEdit)
    {
        this.ModalTitle = title;
        this.IsEditing = isEdit;
        this.ErrorMessage = null;
        this.ShowModal = true;
    }

    private sealed class ReverseStringComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            var result = StringComparer.OrdinalIgnoreCase.Compare(x, y);
            return -result;
        }
    }
}
