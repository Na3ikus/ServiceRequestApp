using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin tags section: table with inline edit (colour presets, live preview) and a delete prompt.
/// </summary>
public partial class TagsTab : BaseComponent
{
    private const string DefaultColor = "#3B82F6";

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

    private ElementReference firstField;

    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public IList<Tag>? Tags { get; set; }

    [Parameter]
    public EventCallback OnTagsChanged { get; set; }

    [Inject]
    protected ITagService TagService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected Tag EditingTag { get; set; } = new ();

    protected RowState Row { get; } = new ();

    protected TableSort Sort { get; } = new ("name");

    protected string SearchQuery { get; set; } = string.Empty;

    protected IList<Tag>? GetFilteredTags()
    {
        var query = this.Tags?
            .Where(t =>
                string.IsNullOrWhiteSpace(this.SearchQuery) ||
                t.Name.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase));

        if (query is null)
        {
            return null;
        }

        var ordered = this.Sort.Column == "count"
            ? this.Sort.Apply(query, t => t.Tickets?.Count ?? 0)
            : this.Sort.Apply(query, t => t.Name, StringComparer.OrdinalIgnoreCase);

        return ordered.ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    protected void SortBy(string column) => this.Sort.Toggle(column);

    protected void ClearSearch() => this.SearchQuery = string.Empty;

    protected void ShowAddTag()
    {
        this.EditingTag = new Tag { Color = DefaultColor };
        this.Row.Add();
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
        this.Row.Edit(tag.Id);
    }

    protected async Task DeleteTagAsync(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        this.Row.Begin();
        try
        {
            var success = await this.TagService.DeleteTagAsync(tag.Id, this.AuthService.CurrentUser?.Id);
            this.Row.Close();
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("tags.deleted"), ToastType.Success);
                await this.OnTagsChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            this.Row.Close();
            await this.ToastService.ShowToastAsync($"Error deleting tag: {ex.Message}", ToastType.Error);
        }
    }

    protected async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(this.EditingTag.Name))
        {
            this.Row.Fail(this.L.Translate("tags.nameRequired"));
            return;
        }

        var isNew = this.Row.IsAdding;
        this.Row.Begin();
        try
        {
            if (isNew)
            {
                await this.TagService.CreateTagAsync(this.EditingTag.Name, this.EditingTag.Color, this.AuthService.CurrentUser?.Id);
                await this.ToastService.ShowToastAsync(this.L.Translate("tags.created"), ToastType.Success);
            }
            else
            {
                await this.TagService.UpdateTagAsync(this.EditingTag.Id, this.EditingTag.Name, this.EditingTag.Color, this.AuthService.CurrentUser?.Id);
                await this.ToastService.ShowToastAsync(this.L.Translate("tags.updated"), ToastType.Success);
            }

            this.Row.Close();
            await this.OnTagsChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            this.Row.Fail(ex.Message);
        }
    }
}
