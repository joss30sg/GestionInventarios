using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;
        private const int LOW_STOCK_THRESHOLD = 5;

        public ReportService(
            ApplicationDbContext context,
            ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LowStockReportItemDto>> GetLowStockProductsAsync()
        {
            try
            {
                _logger.LogInformation("[REPORT] Obteniendo productos con inventario bajo");

                var lowStockProducts = await _context.Products
                    .Where(p => p.IsActive && p.Quantity < LOW_STOCK_THRESHOLD)
                    .OrderBy(p => p.Quantity)
                    .Select(p => new LowStockReportItemDto
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Category = p.Category,
                        CurrentQuantity = p.Quantity,
                        ReorderLevel = LOW_STOCK_THRESHOLD,
                        UnitPrice = p.Price,
                        TotalValue = p.Price * p.Quantity,
                        Status = p.Quantity == 0 ? "Critical" : p.Quantity <= 2 ? "Warning" : "Low",
                        LastUpdated = p.UpdatedAt ?? p.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("[REPORT] Se encontraron {Count} productos con inventario bajo", lowStockProducts.Count);
                return lowStockProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REPORT] Error al obtener productos con inventario bajo");
                throw;
            }
        }

        public async Task<LowStockReportSummaryDto> GetReportSummaryAsync(string userName)
        {
            try
            {
                var lowStockProducts = await GetLowStockProductsAsync();

                return new LowStockReportSummaryDto
                {
                    TotalProductsLowStock = lowStockProducts.Count,
                    CriticalCount = lowStockProducts.Count(p => p.Status == "Critical"),
                    WarningCount = lowStockProducts.Count(p => p.Status == "Warning"),
                    LowCount = lowStockProducts.Count(p => p.Status == "Low"),
                    TotalValue = lowStockProducts.Sum(p => p.TotalValue),
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REPORT] Error al obtener resumen");
                throw;
            }
        }

        public async Task<byte[]> GenerateLowStockPdfReportAsync(string userName)
        {
            try
            {
                _logger.LogInformation("[REPORT] Iniciando generacion de PDF para usuario: {UserName}", userName);

                var lowStockProducts = await GetLowStockProductsAsync();
                var summary = await GetReportSummaryAsync(userName);

                using var memoryStream = new MemoryStream();
                using (var pdfWriter = new PdfWriter(memoryStream))
                using (var pdfDocument = new PdfDocument(pdfWriter))
                {
                    var document = new Document(pdfDocument);

                    var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    document.Add(new Paragraph("REPORTE DE INVENTARIO BAJO")
                        .SetFont(titleFont).SetFontSize(20)
                        .SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(10));

                    document.Add(new Paragraph($"Generado: {summary.GeneratedDate:dd/MM/yyyy HH:mm:ss} | Por: {summary.GeneratedBy}")
                        .SetFont(normalFont).SetFontSize(10)
                        .SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(20)
                        .SetFontColor(ColorConstants.GRAY));

                    document.Add(CreateSummaryTable(summary, headerFont, normalFont));
                    document.Add(new Paragraph("\n"));

                    if (lowStockProducts.Count > 0)
                    {
                        document.Add(CreateProductsTable(lowStockProducts, headerFont, normalFont));
                    }
                    else
                    {
                        document.Add(new Paragraph("No hay productos con inventario bajo en este momento.")
                            .SetFont(normalFont).SetTextAlignment(TextAlignment.CENTER)
                            .SetFontColor(ColorConstants.GRAY).SetMarginTop(20));
                    }

                    document.Add(new Paragraph("\n"));
                    document.Add(new Paragraph("Este reporte es confidencial y solo debe ser accesible para administradores.")
                        .SetFont(normalFont).SetFontSize(8)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontColor(ColorConstants.GRAY).SetMarginTop(30));

                    document.Close();
                }

                _logger.LogInformation("[REPORT] PDF generado exitosamente");
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REPORT] Error al generar PDF");
                throw;
            }
        }

        private Table CreateSummaryTable(LowStockReportSummaryDto summary, PdfFont headerFont, PdfFont normalFont)
        {
            var table = new Table(4).SetWidth(UnitValue.CreatePercentValue(100));
            var headerBg = new DeviceRgb(70, 130, 180);

            AddCellToTable(table, "Total Items Bajos", headerFont, headerBg);
            AddCellToTable(table, "Criticos (0)", headerFont, headerBg);
            AddCellToTable(table, "Advertencia (1-2)", headerFont, headerBg);
            AddCellToTable(table, "Bajos (3-4)", headerFont, headerBg);

            AddCellToTable(table, summary.TotalProductsLowStock.ToString(), normalFont, ColorConstants.WHITE, ColorConstants.BLACK);
            AddCellToTable(table, summary.CriticalCount.ToString(), normalFont, ColorConstants.WHITE, ColorConstants.BLACK);
            AddCellToTable(table, summary.WarningCount.ToString(), normalFont, ColorConstants.WHITE, ColorConstants.BLACK);
            AddCellToTable(table, summary.LowCount.ToString(), normalFont, ColorConstants.WHITE, ColorConstants.BLACK);

            return table;
        }

        private Table CreateProductsTable(List<LowStockReportItemDto> products, PdfFont headerFont, PdfFont normalFont)
        {
            var table = new Table(6).SetWidth(UnitValue.CreatePercentValue(100));
            var headerBg = new DeviceRgb(70, 130, 180);

            AddCellToTable(table, "Producto", headerFont, headerBg);
            AddCellToTable(table, "Categoria", headerFont, headerBg);
            AddCellToTable(table, "Cant. Actual", headerFont, headerBg);
            AddCellToTable(table, "Precio Unit.", headerFont, headerBg);
            AddCellToTable(table, "Valor Total", headerFont, headerBg);
            AddCellToTable(table, "Estado", headerFont, headerBg);

            foreach (var product in products)
            {
                AddCellToTable(table, product.ProductName, normalFont, ColorConstants.WHITE, ColorConstants.BLACK);
                AddCellToTable(table, product.Category, normalFont, ColorConstants.WHITE, ColorConstants.BLACK);
                AddCellToTable(table, product.CurrentQuantity.ToString(), normalFont, ColorConstants.WHITE, ColorConstants.BLACK);
                AddCellToTable(table, $"S/. {product.UnitPrice:N2}", normalFont, ColorConstants.WHITE, ColorConstants.BLACK);
                AddCellToTable(table, $"S/. {product.TotalValue:N2}", normalFont, ColorConstants.WHITE, ColorConstants.BLACK);

                var statusColor = GetStatusColor(product.Status);
                var statusCell = new Cell();
                statusCell.Add(new Paragraph(product.Status).SetFont(normalFont).SetFontColor(statusColor));
                statusCell.SetBackgroundColor(ColorConstants.WHITE);
                table.AddCell(statusCell);
            }

            return table;
        }

        private static void AddCellToTable(Table table, string content, PdfFont font, Color backgroundColor, Color? fontColor = null)
        {
            var cell = new Cell();
            cell.Add(new Paragraph(content).SetFont(font).SetFontColor(fontColor ?? ColorConstants.WHITE));
            cell.SetBackgroundColor(backgroundColor);
            cell.SetPadding(8);
            table.AddCell(cell);
        }

        private static string GetStatus(int quantity)
        {
            return quantity == 0 ? "Critical" : quantity <= 2 ? "Warning" : "Low";
        }

        private static Color GetStatusColor(string status)
        {
            return status switch
            {
                "Critical" => ColorConstants.RED,
                "Warning" => new DeviceRgb(255, 165, 0),
                "Low" => new DeviceRgb(255, 215, 0),
                _ => ColorConstants.BLACK
            };
        }
    }
}
