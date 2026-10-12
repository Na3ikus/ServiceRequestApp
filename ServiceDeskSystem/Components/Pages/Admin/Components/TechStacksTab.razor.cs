using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin tech stacks section: table with inline edit and a delete prompt.
/// </summary>
public partial class TechStacksTab : BaseComponent
{
    private ElementReference firstField;

    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public IList<TechStack>? TechStacks { get; set; }

    [Parameter]
    public EventCallback OnTechStacksChanged { get; set; }

    [Inject]
    protected IAdminService AdminService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected TechStack EditingTechStack { get; set; } = new ();

    protected RowState Row { get; } = new ();

    protected TableSort Sort { get; } = new ("name");

    protected string SearchQuery { get; set; } = string.Empty;

    protected IList<TechStack>? GetFilteredStacks()
    {
        var query = this.TechStacks?
            .Where(ts =>
                string.IsNullOrWhiteSpace(this.SearchQuery) ||
                ts.Name.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                ts.Type.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase));

        if (query is null)
        {
            return null;
        }

        var ordered = this.Sort.Column switch
        {
            "type" => this.Sort.Apply(query, ts => ts.Type, StringComparer.OrdinalIgnoreCase),
            "count" => this.Sort.Apply(query, ts => ts.Products?.Count ?? 0),
            _ => this.Sort.Apply(query, ts => ts.Name, StringComparer.OrdinalIgnoreCase),
        };

        return ordered.ThenBy(ts => ts.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    protected void SortBy(string column) => this.Sort.Toggle(column);

    protected void ClearSearch() => this.SearchQuery = string.Empty;

    protected void ShowAddTechStack()
    {
        this.EditingTechStack = new TechStack();
        this.Row.Add();
    }

    protected void EditTechStack(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        this.EditingTechStack = new TechStack
        {
            Id = techStack.Id,
            Name = techStack.Name,
            Type = techStack.Type,
        };
        this.Row.Edit(techStack.Id);
    }

    protected async Task DeleteTechStackAsync(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        if (techStack.Products.Count > 0)
        {
            this.Row.Close();
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteTechStackWithProducts"), ToastType.Error);
            return;
        }

        this.Row.Begin();
        try
        {
            var success = await this.AdminService.DeleteTechStackAsync(techStack.Id);
            this.Row.Close();
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.techStackDeleted"), ToastType.Success);
                await this.OnTechStacksChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            this.Row.Close();
            await this.ToastService.ShowToastAsync($"Error deleting tech stack: {ex.Message}", ToastType.Error);
        }
    }

    protected async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(this.EditingTechStack.Name))
        {
            this.Row.Fail(this.L.Translate("admin.nameRequired"));
            return;
        }

        var isNew = this.Row.IsAdding;
        this.Row.Begin();
        try
        {
            if (isNew)
            {
                await this.AdminService.CreateTechStackAsync(this.EditingTechStack);
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.techStackCreated"), ToastType.Success);
            }
            else
            {
                await this.AdminService.UpdateTechStackAsync(this.EditingTechStack);
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.techStackUpdated"), ToastType.Success);
            }

            this.Row.Close();
            await this.OnTechStacksChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            this.Row.Fail(ex.Message);
        }
    }
}
