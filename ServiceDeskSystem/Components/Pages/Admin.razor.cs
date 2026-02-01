using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.Common;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Services.Admin;
using ServiceDeskSystem.Services.Auth;
using ServiceDeskSystem.Services.Localization;
using ServiceDeskSystem.Services.Theme;

namespace ServiceDeskSystem.Components.Pages;

/// <summary>
/// Admin panel page for managing products, tech stacks, and users.
/// </summary>
public partial class Admin : IDisposable
{
    private bool disposed;

    private readonly List<ToastMessage> toasts = [];

    internal IReadOnlyList<ToastMessage> Toasts => this.toasts;

    [Inject]
    private IAdminService AdminService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private ILocalizationService L { get; set; } = null!;

    [Inject]
    private IThemeService Theme { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private List<Product>? products { get; set; }

    private List<TechStack>? techStacks { get; set; }

    private List<User>? users { get; set; }

    private string activeTab { get; set; } = "products";

    private bool showModal { get; set; }

    private string modalTitle { get; set; } = string.Empty;

    private string modalType { get; set; } = string.Empty;

    private bool isEditing { get; set; }

    private bool isSaving { get; set; }

    private string? errorMessage { get; set; }

    private Product editingProduct { get; set; } = new ();

    private TechStack editingTechStack { get; set; } = new ();

    private bool IsAdmin => this.AuthService.CurrentUser?.Role == "Admin";

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal async Task RemoveToastAsync(ToastMessage toast)
    {
        toast.IsHiding = true;
        await this.InvokeAsync(this.StateHasChanged);
        await Task.Delay(300);
        this.toasts.Remove(toast);
        await this.InvokeAsync(this.StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;

        if (this.IsAdmin)
        {
            await this.LoadDataAsync();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
            this.L.LanguageChanged -= this.OnStateChanged;
            this.Theme.ThemeChanged -= this.OnStateChanged;
        }

        this.disposed = true;
    }

    private static bool CanDeleteUser(User user) =>
        user.Role != "Admin" && user.CreatedTickets.Count == 0 && user.AssignedTickets.Count == 0;

    private static bool CanToggleUserStatus(User user) => user.Role != "Admin";

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
        var success = await this.AdminService.DeleteProductAsync(product.Id);
        if (success)
        {
            await this.ShowToastAsync(this.L.Translate("admin.productDeleted"), ToastType.Success);
            await this.LoadDataAsync();
        }
        else
        {
            await this.ShowToastAsync(this.L.Translate("admin.cannotDeleteProductWithTickets"), ToastType.Error);
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
            await this.ShowToastAsync(this.L.Translate("admin.productUpdated"), ToastType.Success);
        }
        else
        {
            await this.AdminService.CreateProductAsync(this.editingProduct);
            await this.ShowToastAsync(this.L.Translate("admin.productCreated"), ToastType.Success);
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
        if (techStack.Products.Count > 0)
        {
            await this.ShowToastAsync(this.L.Translate("admin.cannotDeleteTechStackWithProducts"), ToastType.Error);
            return;
        }

        var success = await this.AdminService.DeleteTechStackAsync(techStack.Id);
        if (success)
        {
            await this.ShowToastAsync(this.L.Translate("admin.techStackDeleted"), ToastType.Success);
            await this.LoadTechStacksAsync();
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
            await this.ShowToastAsync(this.L.Translate("admin.techStackUpdated"), ToastType.Success);
        }
        else
        {
            await this.AdminService.CreateTechStackAsync(this.editingTechStack);
            await this.ShowToastAsync(this.L.Translate("admin.techStackCreated"), ToastType.Success);
        }
    }

    private async Task UpdateUserRole(int userId, string newRole)
    {
        var success = await this.AdminService.UpdateUserRoleAsync(userId, newRole);
        if (success)
        {
            await this.ShowToastAsync(this.L.Translate("admin.userUpdated"), ToastType.Success);
            await this.LoadUsersAsync();
        }
    }

    private async Task ToggleUserStatus(int userId)
    {
        var user = this.users?.FirstOrDefault(u => u.Id == userId);
        if (user?.Role == "Admin")
        {
            await this.ShowToastAsync(this.L.Translate("admin.cannotDeactivateAdmin"), ToastType.Error);
            return;
        }

        var success = await this.AdminService.ToggleUserActiveStatusAsync(userId);
        if (success)
        {
            await this.ShowToastAsync(this.L.Translate("admin.userUpdated"), ToastType.Success);
            await this.LoadUsersAsync();
        }
    }

    private async Task DeleteUser(User user)
    {
        if (user.Role == "Admin")
        {
            await this.ShowToastAsync(this.L.Translate("admin.cannotDeleteAdmin"), ToastType.Error);
            return;
        }

        if (user.CreatedTickets.Count > 0 || user.AssignedTickets.Count > 0)
        {
            await this.ShowToastAsync(this.L.Translate("admin.cannotDeleteUserWithTickets"), ToastType.Error);
            return;
        }

        var success = await this.AdminService.DeleteUserAsync(user.Id);
        if (success)
        {
            await this.ShowToastAsync(this.L.Translate("admin.userDeleted"), ToastType.Success);
            await this.LoadUsersAsync();
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

    private async Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 4000)
    {
        var toast = new ToastMessage { Message = message, Type = type };
        this.toasts.Add(toast);
        await this.InvokeAsync(this.StateHasChanged);

        await Task.Delay(durationMs);

        toast.IsHiding = true;
        await this.InvokeAsync(this.StateHasChanged);

        await Task.Delay(300);
        this.toasts.Remove(toast);
        await this.InvokeAsync(this.StateHasChanged);
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);
}
