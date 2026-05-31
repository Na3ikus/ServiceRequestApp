using System.Net.Mail;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Admin.Interfaces;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Toasts.Interfaces;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.Features;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Components.Pages.Admin;

/// <summary>
/// Admin panel page for managing products, tech stacks, and users.
/// </summary>
#pragma warning disable CA1724 // The type name Admin conflicts with the namespace name
public partial class Admin : BaseComponent
{
    #pragma warning restore CA1724
    [Inject]
    private IAdminService AdminService { get; set; } = null!;
    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private IEmailSender EmailSender { get; set; } = null!;

    private List<Product>? products { get; set; }

    private List<TechStack>? techStacks { get; set; }

    private List<User>? users { get; set; }

    private string activeTab { get; set; } = "products";

    private bool isQuickAdminMenuOpen { get; set; }

    private bool showModal { get; set; }

    private string modalTitle { get; set; } = string.Empty;

    private string modalType { get; set; } = string.Empty;

    private bool isEditing { get; set; }

    private bool isSaving { get; set; }

    private string? errorMessage { get; set; }

    private Product editingProduct { get; set; } = new Product();

    private TechStack editingTechStack { get; set; } = new TechStack();

    private string smtpTestRecipient { get; set; } = string.Empty;

    private string smtpTestSubject { get; set; } = "ServiceDesk SMTP test";

    private bool isCheckingSmtp { get; set; }

    private bool isSendingTestEmail { get; set; }

    private bool? smtpCheckSuccess { get; set; }

    private string? smtpCheckMessage { get; set; }

    private bool? smtpSendSuccess { get; set; }

    private string? smtpSendMessage { get; set; }

    private bool IsAdmin => this.AuthService.CurrentUser?.Role == "Admin";

    protected override async Task OnInitializedAsync()
    {
        if (this.IsAdmin)
        {
            await this.LoadDataAsync();
            await this.CheckSmtpStatusAsync(showToast: false);
        }
    }

    private static bool CanDeleteUser(User user) => user.Role != "Admin";

    private static bool CanToggleUserStatus(User user) => user.Role != "Admin";

    private bool CanEditUserRole(User user) => user.Id != this.AuthService.CurrentUser?.Id;

    private async Task LoadDataAsync()
    {
        if (!this.IsAdmin)
        {
            return;
        }

        this.techStacks = await this.AdminService.GetAllTechStacksAsync();
        this.products = await this.AdminService.GetAllProductsAsync();
        this.users = await this.AdminService.GetAllUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        if (!this.IsAdmin)
        {
            return;
        }

        this.users = await this.AdminService.GetAllUsersAsync();
    }

    private async Task LoadTechStacksAsync()
    {
        if (!this.IsAdmin)
        {
            return;
        }

        this.techStacks = await this.AdminService.GetAllTechStacksAsync();
    }

    private void SetActiveTab(string tab) => this.activeTab = tab;

    private void ToggleQuickAdminMenu() => this.isQuickAdminMenuOpen = !this.isQuickAdminMenuOpen;

    private void CloseQuickAdminMenu() => this.isQuickAdminMenuOpen = false;

    private void SetActiveTabFromQuickMenu(string tab)
    {
        this.activeTab = tab;
        this.isQuickAdminMenuOpen = false;
    }

    private async Task CheckSmtpAsync()
    {
        await this.CheckSmtpStatusAsync(showToast: true);
    }

    private async Task CheckSmtpStatusFromCardAsync()
    {
        await this.CheckSmtpStatusAsync(showToast: false);
    }

    private async Task CheckSmtpStatusAsync(bool showToast)
    {
        if (this.isCheckingSmtp)
        {
            return;
        }

        this.isCheckingSmtp = true;
        this.smtpCheckMessage = null;

        try
        {
            var (isSuccess, message) = await this.EmailSender.CheckConnectionAsync().ConfigureAwait(false);
            this.smtpCheckSuccess = isSuccess;
            this.smtpCheckMessage = message;

            if (showToast)
            {
                await this.ToastService.ShowToastAsync(
                    isSuccess ? "SMTP connection is healthy." : $"SMTP check failed: {message}",
                    isSuccess ? ToastType.Success : ToastType.Error).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            this.smtpCheckSuccess = false;
            this.smtpCheckMessage = ex.Message;

            if (showToast)
            {
                await this.ToastService.ShowToastAsync($"SMTP check error: {ex.Message}", ToastType.Error).ConfigureAwait(false);
            }
        }
        finally
        {
            this.isCheckingSmtp = false;
        }
    }

    private async Task SendSmtpTestEmailAsync()
    {
        if (this.isSendingTestEmail)
        {
            return;
        }

        var recipient = this.smtpTestRecipient.Trim();
        if (string.IsNullOrWhiteSpace(recipient) || !MailAddress.TryCreate(recipient, out _))
        {
            this.smtpSendSuccess = false;
            this.smtpSendMessage = "Enter a valid recipient email.";
            await this.ToastService.ShowToastAsync(this.smtpSendMessage, ToastType.Warning).ConfigureAwait(false);
            return;
        }

        this.isSendingTestEmail = true;
        this.smtpSendMessage = null;

        try
        {
            var subject = string.IsNullOrWhiteSpace(this.smtpTestSubject) ? "ServiceDesk SMTP test" : this.smtpTestSubject.Trim();
            var utcNow = DateTime.UtcNow;
            var textBody = $"SMTP test email from ServiceDeskSystem at {utcNow:O}.";
            var htmlBody = $"<p><strong>SMTP test email</strong> from ServiceDeskSystem.</p><p>UTC: {utcNow:O}</p>";

            await this.EmailSender.SendAsync(recipient, subject, htmlBody, textBody).ConfigureAwait(false);

            this.smtpSendSuccess = true;
            this.smtpSendMessage = "Test email sent successfully.";
            await this.ToastService.ShowToastAsync(this.smtpSendMessage, ToastType.Success).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.smtpSendSuccess = false;
            this.smtpSendMessage = ex.Message;
            await this.ToastService.ShowToastAsync($"Test email failed: {ex.Message}", ToastType.Error).ConfigureAwait(false);
        }
        finally
        {
            this.isSendingTestEmail = false;
        }
    }

    private void ShowAddProduct()
    {
        this.editingProduct = new Product { TechStackId = this.techStacks?.FirstOrDefault()?.Id ?? 0 };
        this.OpenModal(this.L.Translate("admin.addProduct"), "product", isEdit: false);
    }

    private void EditProduct(Product product)
    {
        this.editingProduct = new Product
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CurrentVersion = product.CurrentVersion,
            TechStackId = product.TechStackId,
        };
        this.OpenModal(this.L.Translate("admin.editProduct"), "product", isEdit: true);
    }

    private async Task DeleteProduct(Product product)
    {
        try
        {
            var success = await this.AdminService.DeleteProductAsync(product.Id);
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.productDeleted"), ToastType.Success);
                await this.LoadDataAsync();
            }
            else
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteProductWithTickets"), ToastType.Error);
            }
        }
        catch (Exception ex)
        {
            await this.ToastService.ShowToastAsync($"Error deleting product: {ex.Message}", ToastType.Error);
        }
    }

    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(this.editingProduct.Name))
        {
            this.errorMessage = this.L.Translate("admin.nameRequired");
            return;
        }

        if (this.isEditing)
        {
            await this.AdminService.UpdateProductAsync(this.editingProduct);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.productUpdated"), ToastType.Success);
        }
        else
        {
            await this.AdminService.CreateProductAsync(this.editingProduct);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.productCreated"), ToastType.Success);
        }
    }

    private void ShowAddTechStack()
    {
        this.editingTechStack = new TechStack();
        this.OpenModal(this.L.Translate("admin.addTechStack"), "techstack", isEdit: false);
    }

    private void EditTechStack(TechStack techStack)
    {
        this.editingTechStack = new TechStack
        {
            Id = techStack.Id,
            Name = techStack.Name,
            Type = techStack.Type,
        };
        this.OpenModal(this.L.Translate("admin.editTechStack"), "techstack", isEdit: true);
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
                await this.LoadTechStacksAsync();
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

    private async Task UpdateUserRole(int userId, string newRole)
    {
        var user = this.users?.FirstOrDefault(u => u.Id == userId);
        if (user is not null && !this.CanEditUserRole(user))
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotEditSelfRole"), ToastType.Error);
            await this.LoadUsersAsync();
            return;
        }

        var success = await this.AdminService.UpdateUserRoleAsync(userId, newRole);
        if (success)
        {
            if (user is not null)
            {
                user.Role = newRole;
                await this.InvokeAsync(this.StateHasChanged);
            }

            await this.ToastService.ShowToastAsync(this.L.Translate("admin.userUpdated"), ToastType.Success);
        }
    }

    private async Task ToggleUserStatus(int userId)
    {
        var user = this.users?.FirstOrDefault(u => u.Id == userId);
        if (user?.Role == "Admin")
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeactivateAdmin"), ToastType.Error);
            return;
        }

        var success = await this.AdminService.ToggleUserActiveStatusAsync(userId);
        if (success && user is not null)
        {
            user.IsActive = !user.IsActive;
            await this.InvokeAsync(this.StateHasChanged);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.userUpdated"), ToastType.Success);
        }
    }

    private async Task DeleteUser(User user)
    {
        try
        {
            if (user.Role == "Admin")
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteAdmin"), ToastType.Error);
                return;
            }

            var success = await this.AdminService.DeleteUserAsync(user.Id);
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.userDeleted"), ToastType.Success);
                this.users?.Remove(user);
                await this.InvokeAsync(this.StateHasChanged);
            }
            else
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteUserWithTickets"), ToastType.Error);
            }
        }
        catch (Exception ex)
        {
            await this.ToastService.ShowToastAsync($"Error deleting user: {ex.Message}", ToastType.Error);
        }
    }

    private void OpenModal(string title, string type, bool isEdit)
    {
        this.modalTitle = title;
        this.modalType = type;
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
            switch (this.modalType)
            {
                case "product":
                    await this.SaveProductAsync();
                    break;
                case "techstack":
                    await this.SaveTechStackAsync();
                    break;
            }

            if (this.errorMessage is null)
            {
                await this.LoadDataAsync();
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
