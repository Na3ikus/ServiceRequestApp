using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Admin;

public interface IAdminService
{
    Task<List<TechStack>> GetAllTechStacksAsync();

    Task<List<Product>> GetAllProductsAsync();

    Task<TechStack> CreateTechStackAsync(TechStack techStack);

    Task<Product> CreateProductAsync(Product product);

    Task<bool> UpdateTechStackAsync(TechStack techStack);

    Task<bool> UpdateProductAsync(Product product);

    Task<bool> DeleteTechStackAsync(int id);

    Task<bool> DeleteProductAsync(int id);

    // User Management
    Task<List<User>> GetAllUsersAsync();

    Task<bool> UpdateUserRoleAsync(int userId, string newRole);

    Task<bool> ToggleUserActiveStatusAsync(int userId);

    Task<bool> DeleteUserAsync(int userId);
}
