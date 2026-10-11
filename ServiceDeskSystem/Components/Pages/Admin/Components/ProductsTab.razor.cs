using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin products section: table with inline edit and a delete prompt.
/// </summary>
public partial class ProductsTab : BaseComponent
{
    private ElementReference firstField;

    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public IList<Product>? Products { get; set; }

    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public IList<TechStack>? TechStacks { get; set; }

    [Parameter]
    public EventCallback OnProductsChanged { get; set; }

    [Inject]
    protected IAdminService AdminService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected Product EditingProduct { get; set; } = new ();

    protected RowState Row { get; } = new ();

    protected TableSort Sort { get; } = new ("name");

    protected string SearchQuery { get; set; } = string.Empty;

    protected IList<Product>? GetFilteredProducts()
    {
        var query = this.Products?
            .Where(p =>
                string.IsNullOrWhiteSpace(this.SearchQuery) ||
                p.Name.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (p.TechStack?.Name ?? string.Empty).Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.CurrentVersion.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase));

        if (query is null)
        {
            return null;
        }

        Func<Product, string> key = this.Sort.Column switch
        {
            "version" => p => p.CurrentVersion,
            "techstack" => p => p.TechStack?.Name ?? string.Empty,
            _ => p => p.Name,
        };

        return this.Sort.Apply(query, key, StringComparer.OrdinalIgnoreCase).ToList();
    }

    protected void SortBy(string column) => this.Sort.Toggle(column);

    protected void ClearSearch() => this.SearchQuery = string.Empty;

    protected void ShowAddProduct()
    {
        this.EditingProduct = new Product { TechStackId = this.TechStacks?.FirstOrDefault()?.Id ?? 0 };
        this.Row.Add();
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
        this.Row.Edit(product.Id);
    }

    protected async Task DeleteProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        this.Row.Begin();
        try
        {
            var success = await this.AdminService.DeleteProductAsync(product.Id);
            this.Row.Close();
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
            this.Row.Close();
            await this.ToastService.ShowToastAsync($"Error deleting product: {ex.Message}", ToastType.Error);
        }
    }

    protected async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(this.EditingProduct.Name))
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
                await this.AdminService.CreateProductAsync(this.EditingProduct);
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.productCreated"), ToastType.Success);
            }
            else
            {
                await this.AdminService.UpdateProductAsync(this.EditingProduct);
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.productUpdated"), ToastType.Success);
            }

            this.Row.Close();
            await this.OnProductsChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            this.Row.Fail(ex.Message);
        }
    }
}
