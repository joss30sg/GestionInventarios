using InventoryManagementAPI.Application.DTOs.Product;

namespace InventoryManagementAPI.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductResponse>> GetAllProductsAsync();
    Task<ProductResponse?> GetProductByIdAsync(int id);
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
    Task<ProductResponse?> UpdateProductAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
}
