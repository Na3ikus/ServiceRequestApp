
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

        var diff = new List<AuditDiffItem>();
        if (existing.Name != techStack.Name)
        {
            diff.Add(new("Name", existing.Name, techStack.Name));
        }
        if (existing.Type != techStack.Type)
        {
            diff.Add(new("Type", existing.Type.ToString(), techStack.Type.ToString()));
        }

        existing.Name = techStack.Name;
        existing.Type = techStack.Type;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearCache();

        var payload = new AuditChangePayload
        {
            Summary = $"Updated tech stack: {techStack.Name}",
            Severity = "Info",
            Diff = diff.Count > 0 ? diff : null,
        };

        await auditService.LogActionSafeAsync("UPDATE_TECH_STACK", "TechStack", techStack.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), payload.ToJson()).ConfigureAwait(false);

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

        var diff = new List<AuditDiffItem>();
        if (existing.Name != product.Name)
        {
            diff.Add(new("Name", existing.Name, product.Name));
        }
        if (existing.Description != product.Description)
        {
            diff.Add(new("Description", existing.Description, product.Description));
        }
        if (existing.CurrentVersion != product.CurrentVersion)
        {
            diff.Add(new("Version", existing.CurrentVersion, product.CurrentVersion));
        }
        if (existing.TechStackId != product.TechStackId)
        {
            diff.Add(new("TechStackId", existing.TechStackId.ToString(System.Globalization.CultureInfo.InvariantCulture), product.TechStackId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.CurrentVersion = product.CurrentVersion;
        existing.TechStackId = product.TechStackId;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearCache();

        var payload = new AuditChangePayload
        {
            Summary = $"Updated product: {product.Name}",
            Severity = "Info",
            Diff = diff.Count > 0 ? diff : null,
        };

        await auditService.LogActionSafeAsync("UPDATE_PRODUCT", "Product", product.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), payload.ToJson()).ConfigureAwait(false);

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

        var payload = new AuditChangePayload
        {
            Summary = $"Deleted tech stack: {techStack.Name}",
            Severity = "Critical",
        };

        await auditService.LogActionSafeAsync("DELETE_TECH_STACK", "TechStack", id.ToString(System.Globalization.CultureInfo.InvariantCulture), payload.ToJson()).ConfigureAwait(false);

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

        var payload = new AuditChangePayload
        {
            Summary = $"Deleted product: {product.Name}",
            Severity = "Critical",
        };

        await auditService.LogActionSafeAsync("DELETE_PRODUCT", "Product", id.ToString(System.Globalization.CultureInfo.InvariantCulture), payload.ToJson()).ConfigureAwait(false);

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

        var diff = new List<AuditDiffItem>
        {
            new("Role", user.Role.ToString(), newRole.ToString()),
        };

        user.Role = newRole;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        var payload = new AuditChangePayload
        {
            Summary = $"Updated role for user '{user.Login}' to {newRole}",
            Severity = "Warning",
            Diff = diff,
        };

        await auditService.LogActionSafeAsync("UPDATE_USER_ROLE", "User", userId.ToString(System.Globalization.CultureInfo.InvariantCulture), payload.ToJson()).ConfigureAwait(false);

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

        var oldStatus = user.IsActive;
        user.IsActive = !user.IsActive;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        var actionStr = user.IsActive ? "ACTIVATE_USER" : "DEACTIVATE_USER";
        var detailStr = user.IsActive ? $"Activated user account '{user.Login}'" : $"Deactivated user account '{user.Login}'";
        var diff = new List<AuditDiffItem>
        {
            new("IsActive", oldStatus.ToString(), user.IsActive.ToString()),
        };

        var payload = new AuditChangePayload
        {
            Summary = detailStr,
            Severity = user.IsActive ? "Info" : "Warning",
            Diff = diff,
        };

        await auditService.LogActionSafeAsync(actionStr, "User", userId.ToString(System.Globalization.CultureInfo.InvariantCulture), payload.ToJson()).ConfigureAwait(false);

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

        var payload = new AuditChangePayload
        {
            Summary = $"Deleted user: {user.Login}",
            Severity = "Critical",
        };

        await auditService.LogActionSafeAsync("DELETE_USER", "User", userId.ToString(System.Globalization.CultureInfo.InvariantCulture), payload.ToJson()).ConfigureAwait(false);

        return true;
    }
}



