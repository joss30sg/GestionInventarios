using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Application.Interfaces
{
    /// <summary>
    /// DTO para productos con inventario bajo en reportes
    /// </summary>
    public class LowStockReportItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public int CurrentQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; } // "Critical", "Warning", "Low"
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO para resumen del reporte
    /// </summary>
    public class LowStockReportSummaryDto
    {
        public int TotalProductsLowStock { get; set; }
        public int CriticalCount { get; set; }
        public int WarningCount { get; set; }
        public int LowCount { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
    }

    /// <summary>
    /// Servicio para generación de reportes
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Obtiene lista de productos con inventario bajo para el reporte
        /// </summary>
        Task<List<LowStockReportItemDto>> GetLowStockProductsAsync();

        /// <summary>
        /// Obtiene resumen del reporte
        /// </summary>
        Task<LowStockReportSummaryDto> GetReportSummaryAsync(string userName);

        /// <summary>
        /// Genera un reporte PDF de productos con inventario bajo
        /// </summary>
        /// <returns>Array de bytes del PDF generado</returns>
        Task<byte[]> GenerateLowStockPdfReportAsync(string userName);
    }
}
