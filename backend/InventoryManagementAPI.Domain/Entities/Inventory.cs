namespace InventoryManagementAPI.Domain.Entities;

/// <summary>
/// Controla el stock disponible de un producto
/// Cada producto tiene un registro de inventario asociado
/// </summary>
public class Inventory
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID del producto asociado (1:1 relationship)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Stock físicamente disponible en almacén
    /// </summary>
    public int QuantityOnHand { get; set; }
    
    /// <summary>
    /// Stock reservado por pedidos no completados
    /// </summary>
    public int QuantityReserved { get; set; }
    
    /// <summary>
    /// Stock en tránsito (compras a proveedores no recibidas)
    /// </summary>
    public int QuantityOnOrder { get; set; }
    
    /// <summary>
    /// Stock mínimo antes de generar alerta.
    /// Cuando QuantityOnHand <= ReorderLevel, se sugiere recompra
    /// </summary>
    public int ReorderLevel { get; set; }
    
    /// <summary>
    /// Cantidad sugerida a comprar cuando se alcanza ReorderLevel
    /// </summary>
    public int ReorderQuantity { get; set; }
    
    /// <summary>
    /// Último conteo físico de inventario
    /// </summary>
    public DateTime? LastCountedAt { get; set; }
    
    /// <summary>
    /// Fecha del último movimiento de inventario
    /// </summary>
    public DateTime? LastMovementAt { get; set; }
    
    /// <summary>
    /// Calcula la cantidad disponible real (OnHand - Reserved)
    /// </summary>
    public int GetAvailableQuantity() => QuantityOnHand - QuantityReserved;
    
    /// <summary>
    /// Indica si el stock está debajo del nivel de reorden
    /// </summary>
    public bool IsLowStock() => QuantityOnHand <= ReorderLevel;
    
    /// <summary>
    /// Indica si el producto está sin stock
    /// </summary>
    public bool IsOutOfStock() => QuantityOnHand <= 0;
}
