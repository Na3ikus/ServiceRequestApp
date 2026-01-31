using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services.Admin;

internal interface IAdminService
{
    Task<List<TechStack>> GetAllTechStacksAsync();

    Task<List<Product>> GetAllProductsAsync();

    Task<TechStack> CreateTechStackAsync(TechStack techStack);

    Task<Product> CreateProductAsync(Product product);

    Task<bool> UpdateTechStackAsync(TechStack techStack);

    Task<bool> UpdateProductAsync(Product product);

    Task<bool> DeleteTechStackAsync(int id);

    Task<bool> DeleteProductAsync(int id);
}
