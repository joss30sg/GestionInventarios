namespace InventoryManagementAPI.Application.DTOs.Inventory;

/// <summary>
/// DTO para respuesta de inventario de un producto
/// </summary>
public class InventoryResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock disponible en almacén
    /// </summary>
    public int QuantityOnHand { get; set; }
    
    /// <summary>
    /// Stock reservado por pedidos
    /// </summary>
    public int QuantityReserved { get; set; }
    
    /// <summary>
    /// Stock en tránsito
    /// </summary>
    public int QuantityOnOrder { get; set; }
    
    /// <summary>
    /// Cantidad realmente disponible para venta
    /// </summary>
    public int AvailableQuantity { get; set; }
    
    /// <summary>
    /// Nivel mínimo para alerta
    /// </summary>
    public int ReorderLevel { get; set; }
    
    /// <summary>
    /// Cantidad a comprar cuando se alcanza mínimo
    /// </summary>
    public int ReorderQuantity { get; set; }
    
    /// <summary>
    /// Estado del inventario: OK, LOW, OUT_OF_STOCK
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Último conteo físico
    /// </summary>
    public DateTime? LastCountedAt { get; set; }
    
    /// <summary>
    /// Último movimiento
    /// </summary>
    public DateTime? LastMovementAt { get; set; }
}

/// <summary>
/// DTO para crear/registrar un movimiento de inventario
/// </summary>
public class CreateStockMovementRequest
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Tipo de movimiento: purchase, sale, adjustment, return, damage, transfer
    /// </summary>
    public string MovementType { get; set; } = string.Empty;
    
    /// <summary>
    /// Cantidad movida
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// ID del documento relacionado (OrderId, etc)
    /// </summary>
    public int? RelatedDocumentId { get; set; }
    
    /// <summary>
    /// Notas del movimiento
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// DTO para ajuste de inventario (PATCH)
/// </summary>
public class CreateInventoryAdjustmentRequest
{
    /// <summary>
    /// ID del producto a ajustar
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Nueva cantidad de stock (resultado del conteo físico)
    /// </summary>
    public int NewQuantity { get; set; }
    
    /// <summary>
    /// Motivo del ajuste
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualizar niveles de reorden
/// </summary>
public class UpdateReorderLevelsRequest
{
    public int ProductId { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
}

/// <summary>
/// DTO para respuesta de movimiento de stock
/// </summary>
public class StockMovementResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    public int? RelatedDocumentId { get; set; }
    public string Notes { get; set; } = string.Empty;
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO para respuesta de ajuste de inventario
/// </summary>
public class InventoryAdjustmentResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int AdjustmentQuantity { get; set; }  // Diferencia
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO para alertas de stock bajo
/// </summary>
public class StockAlertResponse
{
    public class LowStockItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string Urgency { get; set; } = string.Empty; // low, medium, high
    }
    
    public class OutOfStockItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int ReorderQuantity { get; set; }
    }
    
    public List<LowStockItem> LowStockItems { get; set; } = new();
    public List<OutOfStockItem> OutOfStockItems { get; set; } = new();
    public int TotalAlerts { get; set; }
}

/// <summary>
/// DTO para historial de movimientos paginado
/// </summary>
public class PaginatedStockMovementResponse
{
    public List<StockMovementResponse> Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}
