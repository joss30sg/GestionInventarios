using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Domain.Entities;
using InventoryManagementAPI.Infrastructure.Data;
using InventoryManagementAPI.Infrastructure.Hubs;
using InventoryManagementAPI.Infrastructure.Services;

namespace InventoryManagementAPI.Tests.Services;

public class InventoryNotificationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IHubContext<InventoryNotificationHub>> _hubContextMock;
    private readonly Mock<ILogger<InventoryNotificationService>> _loggerMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly InventoryNotificationService _service;

    public InventoryNotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"NotificationTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _hubContextMock = new Mock<IHubContext<InventoryNotificationHub>>();
        _loggerMock = new Mock<ILogger<InventoryNotificationService>>();
        _clientProxyMock = new Mock<IClientProxy>();

        var hubClientsMock = new Mock<IHubClients>();
        hubClientsMock.Setup(c => c.Group("Administrators")).Returns(_clientProxyMock.Object);
        _hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);

        _service = new InventoryNotificationService(_context, _hubContextMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Constructor Validation

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new InventoryNotificationService(null!, _hubContextMock.Object, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_WithNullHubContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new InventoryNotificationService(_context, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("hubContext");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new InventoryNotificationService(_context, _hubContextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    #endregion

    #region CheckAndNotifyLowStockAsync

    [Fact]
    public async Task CheckAndNotifyLowStockAsync_WhenStockBelowThreshold_SendsNotification()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Producto Test", Quantity = 3, Category = "General", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        await _service.CheckAndNotifyLowStockAsync(1, 3);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync("LowStockAlert", It.IsAny<object?[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndNotifyLowStockAsync_WhenStockAboveThreshold_DoesNotSendNotification()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Producto Test", Quantity = 10, Category = "General", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        await _service.CheckAndNotifyLowStockAsync(1, 10);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), default),
            Times.Never);
    }

    [Fact]
    public async Task CheckAndNotifyLowStockAsync_WhenStockIsZero_SendsNotification()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Producto Test", Quantity = 0, Category = "General", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        await _service.CheckAndNotifyLowStockAsync(1, 0);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync("LowStockAlert", It.IsAny<object?[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndNotifyLowStockAsync_WhenProductNotFound_DoesNotSendNotification()
    {
        // Arrange - no products in database

        // Act
        await _service.CheckAndNotifyLowStockAsync(999, 2);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), default),
            Times.Never);
    }

    [Fact]
    public async Task CheckAndNotifyLowStockAsync_WhenStockExactlyAtThreshold_DoesNotNotify()
    {
        // Arrange - threshold is 5, stock exactly 5 should NOT trigger
        _context.Products.Add(new Product { Id = 1, Name = "Producto Test", Quantity = 5, Category = "General", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        await _service.CheckAndNotifyLowStockAsync(1, 5);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), default),
            Times.Never);
    }

    #endregion

    #region GetLowStockProductsAsync

    [Fact]
    public async Task GetLowStockProductsAsync_WithLowStockProducts_ReturnsAlerts()
    {
        // Arrange
        _context.Products.AddRange(
            new Product { Id = 1, Name = "Producto A", Quantity = 2, Category = "Cat1", IsActive = true },
            new Product { Id = 2, Name = "Producto B", Quantity = 0, Category = "Cat2", IsActive = true },
            new Product { Id = 3, Name = "Producto C", Quantity = 50, Category = "Cat3", IsActive = true } // above threshold
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(a => a.CurrentQuantity);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_WithNoLowStock_ReturnsEmptyList()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 1, Name = "Producto X", Quantity = 100, Category = "General", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLowStockProductsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLowStockProductsAsync_SetsCorrectProductName()
    {
        // Arrange
        _context.Products.Add(new Product { Id = 7, Name = "Laptop HP", Quantity = 1, Category = "Electr\u00f3nica", IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].ProductName.Should().Be("Laptop HP");
        result[0].ThresholdQuantity.Should().Be(5);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetLowStockProductsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
