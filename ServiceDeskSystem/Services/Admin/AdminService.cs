using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services.Admin;

internal sealed class AdminService(IDbContextFactory<BugTrackerDbContext> contextFactory) : IAdminService
{
    public async Task<List<TechStack>> GetAllTechStacksAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.TechStacks
                .Include(t => t.Products)
                .OrderBy(t => t.Name)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Products
                .Include(p => p.TechStack)
                .OrderBy(p => p.Name)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }

    public async Task<TechStack> CreateTechStackAsync(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            dbContext.TechStacks.Add(techStack);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return techStack;
        }
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return product;
        }
    }

    public async Task<bool> UpdateTechStackAsync(TechStack techStack)
    {
        ArgumentNullException.ThrowIfNull(techStack);

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var existing = await dbContext.TechStacks.FindAsync(techStack.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return false;
            }

            existing.Name = techStack.Name;
            existing.Type = techStack.Type;
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var existing = await dbContext.Products.FindAsync(product.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return false;
            }

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.CurrentVersion = product.CurrentVersion;
            existing.TechStackId = product.TechStackId;
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }

    public async Task<bool> DeleteTechStackAsync(int id)
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var techStack = await dbContext.TechStacks
                .Include(t => t.Products)
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);

            if (techStack is null)
            {
                return false;
            }

            if (techStack.Products.Count > 0)
            {
                return false;
            }

            dbContext.TechStacks.Remove(techStack);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var product = await dbContext.Products
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.Id == id)
                .ConfigureAwait(false);

            if (product is null)
            {
                return false;
            }

            if (product.Tickets.Count > 0)
            {
                return false;
            }

            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }

    // User Management
    public async Task<List<User>> GetAllUsersAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            return await dbContext.Users
                .Include(u => u.Person)
                .OrderBy(u => u.Login)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
        }
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
