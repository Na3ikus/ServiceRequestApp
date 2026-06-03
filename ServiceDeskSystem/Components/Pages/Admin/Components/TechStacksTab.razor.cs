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
}

