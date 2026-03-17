using Xunit;
using FluentAssertions;
using InventoryManagementAPI.Domain.Entities;

namespace InventoryManagementAPI.Tests.Domain;

public class InventoryEntityTests
{
    [Fact]
    public void GetAvailableQuantity_ReturnsOnHandMinusReserved()
    {
        var inventory = new Inventory { QuantityOnHand = 100, QuantityReserved = 30 };
        inventory.GetAvailableQuantity().Should().Be(70);
    }

    [Fact]
    public void GetAvailableQuantity_WhenAllReserved_ReturnsZero()
    {
        var inventory = new Inventory { QuantityOnHand = 50, QuantityReserved = 50 };
        inventory.GetAvailableQuantity().Should().Be(0);
    }

    [Fact]
    public void IsLowStock_WhenBelowReorderLevel_ReturnsTrue()
    {
        var inventory = new Inventory { QuantityOnHand = 3, ReorderLevel = 5 };
        inventory.IsLowStock().Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WhenAtReorderLevel_ReturnsTrue()
    {
        var inventory = new Inventory { QuantityOnHand = 5, ReorderLevel = 5 };
        inventory.IsLowStock().Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WhenAboveReorderLevel_ReturnsFalse()
    {
        var inventory = new Inventory { QuantityOnHand = 10, ReorderLevel = 5 };
        inventory.IsLowStock().Should().BeFalse();
    }

    [Fact]
    public void IsOutOfStock_WhenQuantityIsZero_ReturnsTrue()
    {
        var inventory = new Inventory { QuantityOnHand = 0 };
        inventory.IsOutOfStock().Should().BeTrue();
    }

    [Fact]
    public void IsOutOfStock_WhenQuantityIsPositive_ReturnsFalse()
    {
        var inventory = new Inventory { QuantityOnHand = 1 };
        inventory.IsOutOfStock().Should().BeFalse();
    }

    [Fact]
    public void IsOutOfStock_WhenQuantityIsNegative_ReturnsTrue()
    {
        var inventory = new Inventory { QuantityOnHand = -1 };
        inventory.IsOutOfStock().Should().BeTrue();
    }
}
