using InventoryManagementAPI.Application.DTOs.Inventory;

namespace InventoryManagementAPI.Application.Interfaces;

/// <summary>
/// Interfaz para operaciones de inventario
/// </summary>
public interface IInventoryService
{
    // CONSULTAS
    /// <summary>
    /// Obtiene todos los inventarios
    /// </summary>
    Task<List<InventoryResponse>> GetAllInventoriesAsync();
    
    /// <summary>
    /// Obtiene inventario de un producto
    /// </summary>
    Task<InventoryResponse?> GetInventoryByProductIdAsync(int productId);
    
    /// <summary>
    /// Obtiene alertas de stock bajo
    /// </summary>
    Task<StockAlertResponse> GetLowStockAlertsAsync();
    
    /// <summary>
    /// Obtiene historial de movimientos con filtros
    /// </summary>
    Task<PaginatedStockMovementResponse> GetStockMovementsAsync(
        int? productId = null,
        string? movementType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20);
    
    /// <summary>
    /// Obtiene disponibilidad de un producto
    /// </summary>
    Task<int> GetAvailableQuantityAsync(int productId);
    
    // MOVIMIENTOS
    /// <summary>
    /// Registra un movimiento de inventario
    /// </summary>
    Task<StockMovementResponse> CreateStockMovementAsync(CreateStockMovementRequest request, string userId);
    
    /// <summary>
    /// Registra un ajuste de inventario (conteo físico)
    /// </summary>
    Task<InventoryAdjustmentResponse> AdjustInventoryAsync(CreateInventoryAdjustmentRequest request, string userId);
    
    /// <summary>
    /// Actualiza los niveles de reorden
    /// </summary>
    Task<InventoryResponse?> UpdateReorderLevelsAsync(UpdateReorderLevelsRequest request, string userId);
    
    // OPERACIONES INTERNAS
    /// <summary>
    /// Crea registro de inventario para un producto nuevo
    /// </summary>
    Task<bool> InitializeInventoryAsync(int productId);
    
    /// <summary>
    /// Reserva stock por un pedido
    /// </summary>
    Task<bool> ReserveStockAsync(int productId, int quantity);
    
    /// <summary>
    /// Libera stock de un pedido cancelado
    /// </summary>
    Task<bool> ReleaseReservedStockAsync(int productId, int quantity);
    
    /// <summary>
    /// Verifica si hay stock suficiente
    /// </summary>
    Task<bool> HasSufficientStockAsync(int productId, int quantity);
}
