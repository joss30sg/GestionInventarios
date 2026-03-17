using System;
using System.Net.Mime;
using System.Threading.Tasks;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InventoryManagementAPI.Api.Controllers
{
    /// <summary>
    /// Controlador para generación de reportes
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportService reportService,
            ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene lista de productos con inventario bajo
        /// </summary>
        /// <returns>Lista de productos con inventario bajo</returns>
        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
            {
                _logger.LogInformation("[REPORTS] GET /api/reports/low-stock - Usuario: {User}", User.Identity?.Name);

                var products = await _reportService.GetLowStockProductsAsync();

                return Ok(new
                {
                    success = true,
                    data = products,
                    count = products.Count,
                    message = "Productos con inventario bajo obtenidos exitosamente",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REPORTS] Error al obtener productos");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener productos con inventario bajo",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Obtiene resumen del reporte
        /// </summary>
        /// <returns>Resumen con estadísticas del bajo inventario</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetReportSummary()
        {
            try
            {
                var userName = User.Identity?.Name ?? "Unknown";
                _logger.LogInformation("[REPORTS] GET /api/reports/summary - Usuario: {User}", userName);

                var summary = await _reportService.GetReportSummaryAsync(userName);

                return Ok(new
                {
                    success = true,
                    data = summary,
                    message = "Resumen de reporte obtenido exitosamente",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REPORTS] Error al obtener resumen");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener resumen del reporte",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Descarga reporte PDF de productos con inventario bajo
        /// </summary>
        /// <returns>Archivo PDF del reporte</returns>
        [HttpGet("low-stock-pdf")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DownloadLowStockReport()
        {
            try
            {
                var userName = User.Identity?.Name ?? "Unknown";
                _logger.LogInformation("[REPORTS] GET /api/reports/low-stock-pdf - Usuario: {User}", userName);

                var pdfContent = await _reportService.GenerateLowStockPdfReportAsync(userName);
                var fileName = $"ReporteInventarioBajo_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                _logger.LogInformation($"[REPORTS] PDF generado exitosamente - {fileName}");

                return File(
                    pdfContent,
                    MediaTypeNames.Application.Pdf,
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REPORTS] Error al descargar PDF");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al generar el reporte PDF",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Health check del servicio de reportes
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                service = "ReportsService",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
