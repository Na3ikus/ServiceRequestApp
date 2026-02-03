using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Data.Repository;

namespace ServiceDeskSystem.Services.Admin;

internal sealed class AdminService(IDbContextFactory<BugTrackerDbContext> contextFactory) : IAdminService
{
    public async Task<List<TechStack>> GetAllTechStacksAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var techStacks = await repo.TechStacks.GetAllWithProductsAsync().ConfigureAwait(false);
        return techStacks.ToList();
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var products = await repo.Products.GetAllWithTechStackAsync().ConfigureAwait(false);
        return products.ToList();
    }

    public async Task<TechStack> CreateTechStackAsync(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        await using var repo = new RepositoryFacade(contextFactory);
        await repo.TechStacks.CreateAsync(techStack).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return techStack;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        await using var repo = new RepositoryFacade(contextFactory);
        await repo.Products.CreateAsync(product).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return product;
    }

    public async Task<bool> UpdateTechStackAsync(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        await using var repo = new RepositoryFacade(contextFactory);
        var existing = await repo.TechStacks.GetByIdAsync(techStack.Id).ConfigureAwait(false);
        if (existing is null)
        {
            return false;
        }

        existing.Name = techStack.Name;
        existing.Type = techStack.Type;
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        await using var repo = new RepositoryFacade(contextFactory);
        var existing = await repo.Products.GetByIdAsync(product.Id).ConfigureAwait(false);
        if (existing is null)
        {
            return false;
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.CurrentVersion = product.CurrentVersion;
        existing.TechStackId = product.TechStackId;
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<bool> DeleteTechStackAsync(int id)
    {
        await using var repo = new RepositoryFacade(contextFactory);
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
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await using var repo = new RepositoryFacade(contextFactory);
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
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var users = await repo.Users.GetAllWithPersonAsync().ConfigureAwait(false);
        return users.ToList();
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole))
        {
            return false;
        }

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var affectedRows = await dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.Role, newRole))
                .ConfigureAwait(false);

            return affectedRows > 0;
        }
    }

    public async Task<bool> ToggleUserActiveStatusAsync(int userId)
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var currentStatus = await dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => (bool?)u.IsActive)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (currentStatus is null)
            {
                return false;
            }

            var affectedRows = await dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IsActive, !currentStatus.Value))
                .ConfigureAwait(false);
            return affectedRows > 0;
        }
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var user = await dbContext.Users
                .Include(u => u.CreatedTickets)
                .Include(u => u.AssignedTickets)
                .FirstOrDefaultAsync(u => u.Id == userId)
                .ConfigureAwait(false);

            if (user is null)
            {
                return false;
            }

            if (user.CreatedTickets.Count > 0 || user.AssignedTickets.Count > 0)
            {
                return false;
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }
}
