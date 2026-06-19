using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

public partial class TechStacksTab : BaseComponent
{
    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public List<TechStack>? TechStacks { get; set; }

    [Parameter]
    public EventCallback OnTechStacksChanged { get; set; }

    [Inject]
    private IAdminService AdminService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private TechStack editingTechStack { get; set; } = new TechStack();

    private bool showModal { get; set; }

    private string modalTitle { get; set; } = string.Empty;

    private bool isEditing { get; set; }

    private bool isSaving { get; set; }

    private string? errorMessage { get; set; }

    private string searchQuery { get; set; } = string.Empty;

    private string sortColumn { get; set; } = "name";

    private bool sortAscending { get; set; } = true;

    private List<TechStack>? FilteredStacks =>
        this.TechStacks?
            .Where(ts =>
                string.IsNullOrWhiteSpace(this.searchQuery) ||
                ts.Name.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase) ||
                ts.Type.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(ts => this.sortColumn switch
            {
                "type" => ts.Type,
                "count" => (ts.Products?.Count ?? 0).ToString("D10"),
                _ => ts.Name,
            }, this.sortAscending ? StringComparer.OrdinalIgnoreCase : new ReverseStringComparer())
            .ToList();

    private void SetSort(string column)
    {
        if (this.sortColumn == column)
        {
            this.sortAscending = !this.sortAscending;
        }
        else
        {
            this.sortColumn = column;
            this.sortAscending = true;
        }
    }

    private string GetSortArrow(string column)
    {
        if (this.sortColumn != column)
        {
            return "↕";
        }

        return this.sortAscending ? "↑" : "↓";
    }

    private static string GetTypeColorClass(string type)
    {
        return type?.ToUpperInvariant() switch
        {
            var t when t != null && (t.Contains("BACKEND", StringComparison.Ordinal) || t.Contains("SERVER", StringComparison.Ordinal) || t.Contains(".NET", StringComparison.Ordinal) || t.Contains("JAVA", StringComparison.Ordinal) || t.Contains("PYTHON", StringComparison.Ordinal) || t.Contains("NODE", StringComparison.Ordinal) || t.Contains("PHP", StringComparison.Ordinal) || t.Contains("GO", StringComparison.Ordinal) || t.Contains("RUST", StringComparison.Ordinal)) => "type-backend",
            var t when t != null && (t.Contains("FRONTEND", StringComparison.Ordinal) || t.Contains("REACT", StringComparison.Ordinal) || t.Contains("VUE", StringComparison.Ordinal) || t.Contains("ANGULAR", StringComparison.Ordinal) || t.Contains("UI", StringComparison.Ordinal) || t.Contains("WEB", StringComparison.Ordinal)) => "type-frontend",
            var t when t != null && (t.Contains("MOBILE", StringComparison.Ordinal) || t.Contains("ANDROID", StringComparison.Ordinal) || t.Contains("IOS", StringComparison.Ordinal) || t.Contains("KOTLIN", StringComparison.Ordinal) || t.Contains("SWIFT", StringComparison.Ordinal)) => "type-mobile",
            var t when t != null && (t.Contains("EMBED", StringComparison.Ordinal) || t.Contains("FIRMWARE", StringComparison.Ordinal) || t.Contains("C++", StringComparison.Ordinal) || t.Contains("HARDWARE", StringComparison.Ordinal)) => "type-embedded",
            var t when t != null && (t.Contains("INFRA", StringComparison.Ordinal) || t.Contains("NETWORK", StringComparison.Ordinal) || t.Contains("DEVOPS", StringComparison.Ordinal) || t.Contains("CLOUD", StringComparison.Ordinal) || t.Contains("DOCKER", StringComparison.Ordinal) || t.Contains("K8S", StringComparison.Ordinal)) => "type-infra",
            var t when t != null && (t.Contains("DATA", StringComparison.Ordinal) || t.Contains("ML", StringComparison.Ordinal) || t.Contains("AI", StringComparison.Ordinal) || t.Contains("ANALYTICS", StringComparison.Ordinal)) => "type-data",
            _ => "type-default",
        };
    }


    private void ShowAddTechStack()
    {
        this.editingTechStack = new TechStack();
        this.OpenModal(this.L.Translate("admin.addTechStack"), isEdit: false);
    }

    private void EditTechStack(TechStack techStack)
    {
        this.editingTechStack = new TechStack
        {
            Id = techStack.Id,
            Name = techStack.Name,
            Type = techStack.Type,
        };
        this.OpenModal(this.L.Translate("admin.editTechStack"), isEdit: true);
    }

    private async Task DeleteTechStack(TechStack techStack)
    {
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

    private async Task SaveTechStackAsync()
    {
        if (string.IsNullOrWhiteSpace(this.editingTechStack.Name))
        {
            this.errorMessage = this.L.Translate("admin.nameRequired");
            return;
        }

        if (this.isEditing)
        {
            await this.AdminService.UpdateTechStackAsync(this.editingTechStack);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.techStackUpdated"), ToastType.Success);
        }
        else
        {
            await this.AdminService.CreateTechStackAsync(this.editingTechStack);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.techStackCreated"), ToastType.Success);
        }
    }

    private void OpenModal(string title, bool isEdit)
    {
        this.modalTitle = title;
        this.isEditing = isEdit;
        this.errorMessage = null;
        this.showModal = true;
    }

    private void CloseModal()
    {
        this.showModal = false;
        this.errorMessage = null;
    }

    private async Task SaveModal()
    {
        this.isSaving = true;
        this.errorMessage = null;

        try
        {
            await this.SaveTechStackAsync();

            if (this.errorMessage is null)
            {
                await this.OnTechStacksChanged.InvokeAsync();
                this.CloseModal();
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = ex.Message;
        }
        finally
        {
            this.isSaving = false;
        }
    }

    private sealed class ReverseStringComparer : IComparer<string>
    {
        public int Compare(string? x, string? y) =>
            StringComparer.OrdinalIgnoreCase.Compare(y, x);
    }
}
