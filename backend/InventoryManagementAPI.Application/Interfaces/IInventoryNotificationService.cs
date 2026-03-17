namespace InventoryManagementAPI.Application.Interfaces;

/// <summary>
/// DTO para alertas de inventario bajo
/// </summary>
public class LowStockAlertDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int CurrentQuantity { get; set; }
    public int ThresholdQuantity { get; set; }
    public string Category { get; set; }
    public DateTime AlertTime { get; set; }
    public string Severity { get; set; } // "Warning", "Critical"
}

/// <summary>
/// Interfaz para el servicio de notificaciones de inventario bajo
/// </summary>
public interface IInventoryNotificationService
{
    /// <summary>
    /// Verifica y notifica sobre productos con stock bajo
    /// </summary>
    Task CheckAndNotifyLowStockAsync(int productId, int currentQuantity);

    /// <summary>
    /// Obtiene todos los productos con stock bajo (< 5 unidades)
    /// </summary>
    Task<List<LowStockAlertDto>> GetLowStockProductsAsync();
}
