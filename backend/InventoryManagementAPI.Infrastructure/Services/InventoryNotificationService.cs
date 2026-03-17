using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InventoryManagementAPI.Infrastructure.Hubs;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Infrastructure.Data;

namespace InventoryManagementAPI.Infrastructure.Services;

/// <summary>
/// Servicio para gestionar notificaciones de inventario bajo en tiempo real
/// </summary>
public class InventoryNotificationService : IInventoryNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<InventoryNotificationHub> _hubContext;
    private readonly ILogger<InventoryNotificationService> _logger;

    private const int LOW_STOCK_THRESHOLD = 5;

    public InventoryNotificationService(
        ApplicationDbContext context,
        IHubContext<InventoryNotificationHub> hubContext,
        ILogger<InventoryNotificationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Verifica si un producto tiene stock bajo y notifica en tiempo real
    /// </summary>
    public async Task CheckAndNotifyLowStockAsync(int productId, int currentQuantity)
    {
        try
        {
            if (currentQuantity < LOW_STOCK_THRESHOLD)
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

                if (product != null)
                {
                    var severity = currentQuantity == 0 ? "Critical" : "Warning";

                    var alert = new LowStockAlertDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        CurrentQuantity = currentQuantity,
                        ThresholdQuantity = LOW_STOCK_THRESHOLD,
                        Category = product.Category,
                        AlertTime = DateTime.UtcNow,
                        Severity = severity
                    };

                    await _hubContext.Clients.Group("Administrators").SendAsync("LowStockAlert", alert);

                    _logger.LogWarning("[ALERT] Stock Bajo: ProductId={ProductId} | Producto={Name} | Stock={Stock} | Umbral={Threshold}", productId, product.Name, currentQuantity, LOW_STOCK_THRESHOLD);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Error al verificar stock bajo para producto ID={ProductId}", productId);
        }
    }

    /// <summary>
    /// Obtiene todos los productos con stock bajo
    /// </summary>
    public async Task<List<LowStockAlertDto>> GetLowStockProductsAsync()
    {
        try
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.IsActive && p.Quantity < LOW_STOCK_THRESHOLD)
                .OrderBy(p => p.Quantity)
                .Select(p => new LowStockAlertDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentQuantity = p.Quantity,
                    ThresholdQuantity = LOW_STOCK_THRESHOLD,
                    Category = p.Category,
                    AlertTime = DateTime.UtcNow,
                    Severity = p.Quantity == 0 ? "Critical" : "Warning"
                })
                .ToListAsync();

            _logger.LogInformation("[INVENTORY] Se encontraron {Count} productos con stock bajo", lowStockProducts.Count);
            return lowStockProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Error al obtener productos con stock bajo");
            return new List<LowStockAlertDto>();
        }
    }
}
