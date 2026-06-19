using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

public partial class ProductsTab : BaseComponent
{
    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public List<Product>? Products { get; set; }

    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public List<TechStack>? TechStacks { get; set; }

    [Parameter]
    public EventCallback OnProductsChanged { get; set; }

    [Inject]
    private IAdminService AdminService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private Product editingProduct { get; set; } = new Product();

    private bool showModal { get; set; }

    private string modalTitle { get; set; } = string.Empty;

    private bool isEditing { get; set; }

    private bool isSaving { get; set; }

    private string? errorMessage { get; set; }

    private string searchQuery { get; set; } = string.Empty;

    private string sortColumn { get; set; } = "name";

    private bool sortAscending { get; set; } = true;

    private List<Product>? FilteredProducts =>
        this.Products?
            .Where(p =>
                string.IsNullOrWhiteSpace(this.searchQuery) ||
                p.Name.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase) ||
                (p.TechStack?.Name ?? string.Empty).Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.CurrentVersion.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => this.sortColumn switch
            {
                "version" => p.CurrentVersion,
                "techstack" => p.TechStack?.Name ?? string.Empty,
                _ => p.Name,
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

    private static string GetStackColorClass(string type)
    {
        return type?.ToUpperInvariant() switch
        {
            var t when t != null && (t.Contains("C#") || t.Contains(".NET") || t.Contains("DOTNET")) => "stack-dotnet",
            var t when t != null && (t.Contains("C++") || t.Contains("EMBED")) => "stack-cpp",
            var t when t != null && (t.Contains("JAVA") && !t.Contains("SCRIPT")) => "stack-java",
            var t when t != null && (t.Contains("JAVASCRIPT") || t.Contains("JS") || t.Contains("NODE") || t.Contains("REACT") || t.Contains("VUE") || t.Contains("ANGULAR")) => "stack-js",
            var t when t != null && (t.Contains("PYTHON") || t.Contains("PY")) => "stack-python",
            var t when t != null && (t.Contains("ANDROID") || t.Contains("KOTLIN") || t.Contains("IOS") || t.Contains("SWIFT") || t.Contains("MOBILE")) => "stack-mobile",
            var t when t != null && (t.Contains("PHP")) => "stack-php",
            var t when t != null && (t.Contains("GO") || t.Contains("RUST")) => "stack-go",
            _ => "stack-default",
        };
    }

    private void ShowAddProduct()
    {
        this.editingProduct = new Product { TechStackId = this.TechStacks?.FirstOrDefault()?.Id ?? 0 };
        this.OpenModal(this.L.Translate("admin.addProduct"), isEdit: false);
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
        this.OpenModal(this.L.Translate("admin.editProduct"), isEdit: true);
    }

    private async Task DeleteProduct(Product product)
    {
        try
        {
            var success = await this.AdminService.DeleteProductAsync(product.Id);
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.productDeleted"), ToastType.Success);
                await this.OnProductsChanged.InvokeAsync();
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
            await this.SaveProductAsync();

            if (this.errorMessage is null)
            {
                await this.OnProductsChanged.InvokeAsync();
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
