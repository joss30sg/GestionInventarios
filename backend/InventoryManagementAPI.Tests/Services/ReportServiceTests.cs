using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Domain.Entities;
using InventoryManagementAPI.Infrastructure.Data;
using InventoryManagementAPI.Infrastructure.Services;

namespace InventoryManagementAPI.Tests.Services;

public class ReportServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<ReportService>> _loggerMock;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"ReportTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<ReportService>>();
        _reportService = new ReportService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetLowStockProductsAsync

    [Fact]
    public async Task GetLowStockProductsAsync_WithLowStockItems_ReturnsCorrectProducts()
    {
        // Arrange
        _context.Products.AddRange(
            new Product { Id = 1, Name = "Prod A", Quantity = 0, Price = 10m, Category = "Cat1", IsActive = true },
            new Product { Id = 2, Name = "Prod B", Quantity = 2, Price = 20m, Category = "Cat2", IsActive = true },
            new Product { Id = 3, Name = "Prod C", Quantity = 4, Price = 30m, Category = "Cat3", IsActive = true },
            new Product { Id = 4, Name = "Prod D", Quantity = 10, Price = 40m, Category = "Cat4", IsActive = true } // above threshold
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(p => p.CurrentQuantity);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_WithNoLowStockItems_ReturnsEmptyList()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Prod X", Quantity = 50, Price = 10m, Category = "General", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetLowStockProductsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLowStockProductsAsync_CriticalStatus_WhenQuantityIsZero()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Prod A", Quantity = 0, Price = 15m, Category = "Cat1", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Critical");
        result[0].CurrentQuantity.Should().Be(0);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_WarningStatus_WhenQuantityIsOneOrTwo()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Prod A", Quantity = 2, Price = 10m, Category = "Cat1", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Warning");
    }

    [Fact]
    public async Task GetLowStockProductsAsync_LowStatus_WhenQuantityIsThreeOrFour()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Prod A", Quantity = 3, Price = 10m, Category = "Cat1", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Low");
    }

    [Fact]
    public async Task GetLowStockProductsAsync_SetsProductNameFromProduct()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 42, Name = "Laptop HP", Quantity = 1, Price = 999m, Category = "Electr\u00f3nica", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].ProductName.Should().Be("Laptop HP");
        result[0].ProductId.Should().Be(42);
        result[0].UnitPrice.Should().Be(999m);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_ReorderLevelDefaultsToThreshold()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Prod A", Quantity = 3, Price = 10m, Category = "Cat1", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetLowStockProductsAsync();

        // Assert
        result[0].ReorderLevel.Should().Be(5); // LOW_STOCK_THRESHOLD default
    }

    [Fact]
    public async Task GetLowStockProductsAsync_CalculatesTotalValue()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Prod A", Quantity = 3, Price = 25.50m, Category = "Cat1", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetLowStockProductsAsync();

        // Assert
        result[0].TotalValue.Should().Be(76.50m); // 25.50 * 3
    }

    #endregion

    #region GetReportSummaryAsync

    [Fact]
    public async Task GetReportSummaryAsync_ReturnsCorrectSummary()
    {
        // Arrange
        _context.Products.AddRange(
            new Product { Id = 1, Name = "A", Quantity = 0, Price = 10m, Category = "C1", IsActive = true },  // Critical
            new Product { Id = 2, Name = "B", Quantity = 1, Price = 20m, Category = "C2", IsActive = true },  // Warning
            new Product { Id = 3, Name = "C", Quantity = 2, Price = 30m, Category = "C3", IsActive = true },  // Warning
            new Product { Id = 4, Name = "D", Quantity = 4, Price = 40m, Category = "C4", IsActive = true }   // Low
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetReportSummaryAsync("admin");

        // Assert
        result.TotalProductsLowStock.Should().Be(4);
        result.CriticalCount.Should().Be(1);
        result.WarningCount.Should().Be(2);
        result.LowCount.Should().Be(1);
        result.GeneratedBy.Should().Be("admin");
        result.GeneratedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetReportSummaryAsync_WithNoLowStock_ReturnsZeroCounts()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Prod X", Quantity = 100, Price = 10m, Category = "General", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetReportSummaryAsync("testuser");

        // Assert
        result.TotalProductsLowStock.Should().Be(0);
        result.CriticalCount.Should().Be(0);
        result.WarningCount.Should().Be(0);
        result.LowCount.Should().Be(0);
        result.GeneratedBy.Should().Be("testuser");
    }

    #endregion

    #region GenerateLowStockPdfReportAsync

    [Fact]
    public async Task GenerateLowStockPdfReportAsync_WithLowStockProducts_GeneratesPdf()
    {
        // Arrange
        _context.Products.AddRange(
            new Product { Id = 1, Name = "Prod A", Quantity = 0, Price = 10m, Category = "Cat1", IsActive = true },
            new Product { Id = 2, Name = "Prod B", Quantity = 3, Price = 20m, Category = "Cat2", IsActive = true }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GenerateLowStockPdfReportAsync("admin");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Length.Should().BeGreaterThan(100); // PDF has real content
        // PDF magic bytes: %PDF
        result[0].Should().Be(0x25); // %
        result[1].Should().Be(0x50); // P
        result[2].Should().Be(0x44); // D
        result[3].Should().Be(0x46); // F
    }

    [Fact]
    public async Task GenerateLowStockPdfReportAsync_WithNoProducts_StillGeneratesPdf()
    {
        // Act
        var result = await _reportService.GenerateLowStockPdfReportAsync("admin");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        // Should still be a valid PDF
        result[0].Should().Be(0x25); // %
    }

    #endregion
}
