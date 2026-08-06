using System.Globalization;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin panel tech stacks management tab component.
/// </summary>
public partial class TechStacksTab : BaseComponent
{
    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public List<TechStack>? TechStacks { get; set; }

    [Parameter]
    public EventCallback OnTechStacksChanged { get; set; }

    [Inject]
    protected IAdminService AdminService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected TechStack EditingTechStack { get; set; } = new ();

    protected bool ShowModal { get; set; }

    protected string ModalTitle { get; set; } = string.Empty;

    protected bool IsEditing { get; set; }

    protected bool IsSaving { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string SearchQuery { get; set; } = string.Empty;

    protected string SortColumn { get; set; } = "name";

    protected bool SortAscending { get; set; } = true;

    protected static string GetTypeColorClass(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return "type-default";
        }

        var t = type.ToUpperInvariant();

        if (t.Contains("BACKEND", StringComparison.OrdinalIgnoreCase) || t.Contains("SERVER", StringComparison.OrdinalIgnoreCase) || t.Contains(".NET", StringComparison.OrdinalIgnoreCase) || t.Contains("JAVA", StringComparison.OrdinalIgnoreCase) || t.Contains("PYTHON", StringComparison.OrdinalIgnoreCase) || t.Contains("NODE", StringComparison.OrdinalIgnoreCase) || t.Contains("PHP", StringComparison.OrdinalIgnoreCase) || t.Contains("GO", StringComparison.OrdinalIgnoreCase) || t.Contains("RUST", StringComparison.OrdinalIgnoreCase))
        {
            return "type-backend";
        }

        if (t.Contains("FRONTEND", StringComparison.OrdinalIgnoreCase) || t.Contains("REACT", StringComparison.OrdinalIgnoreCase) || t.Contains("VUE", StringComparison.OrdinalIgnoreCase) || t.Contains("ANGULAR", StringComparison.OrdinalIgnoreCase) || t.Contains("UI", StringComparison.OrdinalIgnoreCase) || t.Contains("WEB", StringComparison.OrdinalIgnoreCase))
        {
            return "type-frontend";
        }

        if (t.Contains("MOBILE", StringComparison.OrdinalIgnoreCase) || t.Contains("ANDROID", StringComparison.OrdinalIgnoreCase) || t.Contains("IOS", StringComparison.OrdinalIgnoreCase) || t.Contains("KOTLIN", StringComparison.OrdinalIgnoreCase) || t.Contains("SWIFT", StringComparison.OrdinalIgnoreCase))
        {
            return "type-mobile";
        }

        if (t.Contains("EMBED", StringComparison.OrdinalIgnoreCase) || t.Contains("FIRMWARE", StringComparison.OrdinalIgnoreCase) || t.Contains("C++", StringComparison.OrdinalIgnoreCase) || t.Contains("HARDWARE", StringComparison.OrdinalIgnoreCase))
        {
            return "type-embedded";
        }

        if (t.Contains("INFRA", StringComparison.OrdinalIgnoreCase) || t.Contains("NETWORK", StringComparison.OrdinalIgnoreCase) || t.Contains("DEVOPS", StringComparison.OrdinalIgnoreCase) || t.Contains("CLOUD", StringComparison.OrdinalIgnoreCase) || t.Contains("DOCKER", StringComparison.OrdinalIgnoreCase) || t.Contains("K8S", StringComparison.OrdinalIgnoreCase))
        {
            return "type-infra";
        }

        if (t.Contains("DATA", StringComparison.OrdinalIgnoreCase) || t.Contains("ML", StringComparison.OrdinalIgnoreCase) || t.Contains("AI", StringComparison.OrdinalIgnoreCase) || t.Contains("ANALYTICS", StringComparison.OrdinalIgnoreCase))
        {
            return "type-data";
        }

        return "type-default";
    }

    protected List<TechStack>? GetFilteredStacks()
    {
        IComparer<string> comparer = this.SortAscending ? StringComparer.OrdinalIgnoreCase : new ReverseStringComparer();
        return this.TechStacks?
            .Where(ts =>
                string.IsNullOrWhiteSpace(this.SearchQuery) ||
                ts.Name.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                ts.Type.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(
                ts => this.SortColumn switch
                {
                    "type" => ts.Type,
                    "count" => (ts.Products?.Count ?? 0).ToString("D10", CultureInfo.InvariantCulture),
                    _ => ts.Name,
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

    protected void ShowAddTechStack()
    {
        this.EditingTechStack = new TechStack();
        this.OpenModal(this.L.Translate("admin.addTechStack"), isEdit: false);
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
        this.OpenModal(this.L.Translate("admin.editTechStack"), isEdit: true);
    }

    protected async Task DeleteTechStack(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        try
        {
            if (techStack.Products.Count > 0)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteTechStackWithProducts"), ToastType.Error);
                return;
            }

            var success = await this.AdminService.DeleteTechStackAsync(techStack.Id);
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.techStackDeleted"), ToastType.Success);
                await this.OnTechStacksChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            await this.ToastService.ShowToastAsync($"Error deleting tech stack: {ex.Message}", ToastType.Error);
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
            await this.SaveTechStackAsync();

            if (this.ErrorMessage is null)
            {
                await this.OnTechStacksChanged.InvokeAsync();
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

    private async Task SaveTechStackAsync()
    {
        if (string.IsNullOrWhiteSpace(this.EditingTechStack.Name))
        {
            this.ErrorMessage = this.L.Translate("admin.nameRequired");
            return;
        }

        if (this.IsEditing)
        {
            await this.AdminService.UpdateTechStackAsync(this.EditingTechStack);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.techStackUpdated"), ToastType.Success);
        }
        else
        {
            await this.AdminService.CreateTechStackAsync(this.EditingTechStack);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.techStackCreated"), ToastType.Success);
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
