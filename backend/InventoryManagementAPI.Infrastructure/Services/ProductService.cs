using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InventoryManagementAPI.Application.DTOs.Product;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Domain.Entities;
using InventoryManagementAPI.Infrastructure.Data;

namespace InventoryManagementAPI.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    private readonly IInventoryNotificationService _notificationService;

    public ProductService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<ProductService> logger,
        IInventoryNotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<List<ProductResponse>> GetAllProductsAsync()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<ProductResponse>>(products);
    }

    public async Task<ProductResponse?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        return product == null ? null : _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        var product = _mapper.Map<Product>(request);
        product.CreatedAt = DateTime.UtcNow;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("[AUDIT] Producto creado: {Name} | ID={Id}", product.Name, product.Id);

        await _notificationService.CheckAndNotifyLowStockAsync(product.Id, product.Quantity);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse?> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null) return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Quantity = request.Quantity;
        product.Category = request.Category;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("[AUDIT] Producto actualizado: {Name} | ID={Id}", product.Name, product.Id);

        await _notificationService.CheckAndNotifyLowStockAsync(product.Id, product.Quantity);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null) return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("[AUDIT] Producto eliminado (soft): ID={Id}", id);
        return true;
    }
}
