namespace InventoryManagementAPI.Domain.Entities;

/// <summary>
/// Registra cada movimiento de inventario (entrada, salida, ajuste)
/// Proporciona auditoría completa del inventario
/// </summary>
public class StockMovement
{
    public int Id { get; set; }
    
    /// <summary>
    /// Tipo de movimiento realizado
    /// </summary>
    public MovementType Type { get; set; }
    
    /// <summary>
    /// ID del producto afectado
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Cantidad movida (positiva o negativa)
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Stock antes del movimiento
    /// </summary>
    public int BalanceBefore { get; set; }
    
    /// <summary>
    /// Stock después del movimiento
    /// </summary>
    public int BalanceAfter { get; set; }
    
    /// <summary>
    /// ID del documento relacionado (OrderId, PurchaseOrderId, etc.)
    /// </summary>
    public int? RelatedDocumentId { get; set; }
    
    /// <summary>
    /// Notas adicionales del movimiento
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Motivo de ajuste (si aplica)
    /// </summary>
    public string? AdjustmentReason { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Tipos de movimientos de inventario
/// </summary>
public enum MovementType
{
    /// <summary>Compra a proveedor</summary>
    Purchase = 1,
    
    /// <summary>Venta a cliente</summary>
    Sale = 2,
    
    /// <summary>Ajuste manual de inventario</summary>
    Adjustment = 3,
    
    /// <summary>Devolución de cliente</summary>
    Return = 4,
    
    /// <summary>Daño o pérdida</summary>
    Damage = 5,
    
    /// <summary>Transferencia entre almacenes (si aplica)</summary>
    Transfer = 6
}
