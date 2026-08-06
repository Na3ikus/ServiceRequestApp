using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin panel products management tab component.
/// </summary>
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
    protected IAdminService AdminService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected Product EditingProduct { get; set; } = new ();

    protected bool ShowModal { get; set; }

    protected string ModalTitle { get; set; } = string.Empty;

    protected bool IsEditing { get; set; }

    protected bool IsSaving { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string SearchQuery { get; set; } = string.Empty;

    protected string SortColumn { get; set; } = "name";

    protected bool SortAscending { get; set; } = true;

    protected static string GetStackColorClass(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return "stack-default";
        }

        var t = type.ToUpperInvariant();

        if (t.Contains("C#", StringComparison.OrdinalIgnoreCase) || t.Contains(".NET", StringComparison.OrdinalIgnoreCase) || t.Contains("DOTNET", StringComparison.OrdinalIgnoreCase))
        {
            return "stack-dotnet";
        }

        if (t.Contains("C++", StringComparison.OrdinalIgnoreCase) || t.Contains("EMBED", StringComparison.OrdinalIgnoreCase))
        {
            return "stack-cpp";
        }

        if (t.Contains("JAVA", StringComparison.OrdinalIgnoreCase) && !t.Contains("SCRIPT", StringComparison.OrdinalIgnoreCase))
        {
            return "stack-java";
        }

        if (t.Contains("JAVASCRIPT", StringComparison.OrdinalIgnoreCase) || t.Contains("JS", StringComparison.OrdinalIgnoreCase) || t.Contains("NODE", StringComparison.OrdinalIgnoreCase) || t.Contains("REACT", StringComparison.OrdinalIgnoreCase) || t.Contains("VUE", StringComparison.OrdinalIgnoreCase) || t.Contains("ANGULAR", StringComparison.OrdinalIgnoreCase))
        {
            return "stack-js";
        }

        if (t.Contains("PYTHON", StringComparison.OrdinalIgnoreCase) || t.Contains("PY", StringComparison.OrdinalIgnoreCase))
        {
            return "stack-python";
        }

        if (t.Contains("ANDROID", StringComparison.OrdinalIgnoreCase) || t.Contains("KOTLIN", StringComparison.OrdinalIgnoreCase) || t.Contains("IOS", StringComparison.OrdinalIgnoreCase) || t.Contains("SWIFT", StringComparison.OrdinalIgnoreCase) || t.Contains("MOBILE", StringComparison.OrdinalIgnoreCase))
        {
            return "stack-mobile";
        }

        if (t.Contains("PHP", StringComparison.OrdinalIgnoreCase))
        {
            return "stack-php";
        }

        if (t.Contains("GO", StringComparison.OrdinalIgnoreCase) || t.Contains("RUST", StringComparison.OrdinalIgnoreCase))
        {
            return "stack-go";
        }

        return "stack-default";
    }

    protected List<Product>? GetFilteredProducts() =>
        this.Products?
            .Where(p =>
                string.IsNullOrWhiteSpace(this.SearchQuery) ||
                p.Name.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (p.TechStack?.Name ?? string.Empty).Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.CurrentVersion.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(
                p => this.SortColumn switch
            {
                "version" => p.CurrentVersion,
                "techstack" => p.TechStack?.Name ?? string.Empty,
                _ => p.Name,
            }, this.SortAscending ? StringComparer.OrdinalIgnoreCase : new ReverseStringComparer())
            .ToList();

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

    protected void ShowAddProduct()
    {
        this.EditingProduct = new Product { TechStackId = this.TechStacks?.FirstOrDefault()?.Id ?? 0 };
        this.OpenModal(this.L.Translate("admin.addProduct"), isEdit: false);
    }

    protected void EditProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        this.EditingProduct = new Product
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CurrentVersion = product.CurrentVersion,
            TechStackId = product.TechStackId,
        };
        this.OpenModal(this.L.Translate("admin.editProduct"), isEdit: true);
    }

    protected async Task DeleteProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

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
            await this.SaveProductAsync();

            if (this.ErrorMessage is null)
            {
                await this.OnProductsChanged.InvokeAsync();
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

    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(this.EditingProduct.Name))
        {
            this.ErrorMessage = this.L.Translate("admin.nameRequired");
            return;
        }

        if (this.IsEditing)
        {
            await this.AdminService.UpdateProductAsync(this.EditingProduct);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.productUpdated"), ToastType.Success);
        }
        else
        {
            await this.AdminService.CreateProductAsync(this.EditingProduct);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.productCreated"), ToastType.Success);
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
