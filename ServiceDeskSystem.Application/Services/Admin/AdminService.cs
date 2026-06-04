
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace ServiceDeskSystem.Application.Services.Admin;

public sealed class AdminService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    IMemoryCache memoryCache,
    IAuditService? auditService = null) : IAdminService
{
    private void ClearCache()
    {
        memoryCache.Remove("AllTechStacks");
        memoryCache.Remove("AllProducts");
    }

    public async Task<List<TechStack>> GetAllTechStacksAsync()
    {
        return await memoryCache.GetOrCreateAsync("AllTechStacks", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            await using var repo = repositoryFacadeFactory.Create();
            var techStacks = await repo.TechStacks.GetAllWithProductsAsync().ConfigureAwait(false);
            return techStacks.ToList();
        }) ?? [];
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await memoryCache.GetOrCreateAsync("AllProducts", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            await using var repo = repositoryFacadeFactory.Create();
            var products = await repo.Products.GetAllWithTechStackAsync().ConfigureAwait(false);
            return products.ToList();
        }) ?? [];
    }

    public async Task<TechStack> CreateTechStackAsync(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        await using var repo = repositoryFacadeFactory.Create();
        await repo.TechStacks.CreateAsync(techStack).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearCache();

        await auditService.LogActionSafeAsync("CREATE_TECH_STACK", "TechStack", techStack.Id.ToString(), $"Created tech stack: {techStack.Name}").ConfigureAwait(false);

        return techStack;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        await using var repo = repositoryFacadeFactory.Create();
        await repo.Products.CreateAsync(product).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearCache();

        await auditService.LogActionSafeAsync("CREATE_PRODUCT", "Product", product.Id.ToString(), $"Created product: {product.Name}").ConfigureAwait(false);

        return product;
    }

    public async Task<bool> UpdateTechStackAsync(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        await using var repo = repositoryFacadeFactory.Create();
        var existing = await repo.TechStacks.GetByIdAsync(techStack.Id).ConfigureAwait(false);
        if (existing is null)
        {
            return false;
        }

        existing.Name = techStack.Name;
        existing.Type = techStack.Type;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearCache();

        await auditService.LogActionSafeAsync("UPDATE_TECH_STACK", "TechStack", techStack.Id.ToString(), $"Updated tech stack: {techStack.Name}").ConfigureAwait(false);

        return true;
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        await using var repo = repositoryFacadeFactory.Create();
        var existing = await repo.Products.GetByIdAsync(product.Id).ConfigureAwait(false);
        if (existing is null)
        {
            return false;
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.CurrentVersion = product.CurrentVersion;
        existing.TechStackId = product.TechStackId;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearCache();

        await auditService.LogActionSafeAsync("UPDATE_PRODUCT", "Product", product.Id.ToString(), $"Updated product: {product.Name}").ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeleteTechStackAsync(int id)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var techStack = await repo.TechStacks.GetByIdWithProductsAsync(id).ConfigureAwait(false);

        if (techStack is null)
        {
            return false;
        }

        if (techStack.Products.Count > 0)
        {
            return false;
        }

        await repo.TechStacks.DeleteAsync(id).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearCache();

        await auditService.LogActionSafeAsync("DELETE_TECH_STACK", "TechStack", id.ToString(), $"Deleted tech stack: {techStack.Name}").ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var product = await repo.Products.GetByIdWithTicketsAsync(id).ConfigureAwait(false);

        if (product is null)
        {
            return false;
        }

        if (product.Tickets.Count > 0)
        {
            return false;
        }

        await repo.Products.DeleteAsync(id).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearCache();

        await auditService.LogActionSafeAsync("DELETE_PRODUCT", "Product", id.ToString(), $"Deleted product: {product.Name}").ConfigureAwait(false);

        return true;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var users = await repo.Users.GetAllWithPersonAsync().ConfigureAwait(false);
        return users.ToList();
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, UserRole newRole)
    {

        await using var repo = repositoryFacadeFactory.Create();
        var user = await repo.Users.GetByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
        {
            return false;
        }

        user.Role = newRole;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("UPDATE_USER_ROLE", "User", userId.ToString(), $"Updated user role to: {newRole}").ConfigureAwait(false);

        return true;
    }

    public async Task<bool> ToggleUserActiveStatusAsync(int userId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var user = await repo.Users.GetByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
        {
            return false;
        }

        user.IsActive = !user.IsActive;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        var actionStr = user.IsActive ? "ACTIVATE_USER" : "DEACTIVATE_USER";
        var detailStr = user.IsActive ? "Activated user account" : "Deactivated user account";

        await auditService.LogActionSafeAsync(actionStr, "User", userId.ToString(), detailStr).ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var user = await repo.Users.GetByIdAsync(userId).ConfigureAwait(false);
        
        if (user is null)
        {
            return false;
        }

        var hasTickets = await repo.Tickets.HasTicketsForUserAsync(userId).ConfigureAwait(false);
        if (hasTickets)
        {
            return false;
        }

        await repo.Users.DeleteAsync(userId).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("DELETE_USER", "User", userId.ToString(), $"Deleted user: {user.Login}").ConfigureAwait(false);

        return true;
    }
}



