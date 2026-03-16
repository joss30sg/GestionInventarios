using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InventoryManagementAPI.Application.DTOs.Inventory;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Domain.Entities;
using InventoryManagementAPI.Infrastructure.Data;

namespace InventoryManagementAPI.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<InventoryResponse>> GetAllInventoriesAsync()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync();

        var inventories = await _context.Inventories
            .AsNoTracking()
            .ToListAsync();

        var inventoryByProduct = inventories.ToDictionary(i => i.ProductId);

        var result = new List<InventoryResponse>();

        foreach (var product in products)
        {
            if (inventoryByProduct.TryGetValue(product.Id, out var inv))
            {
                result.Add(MapToResponse(inv, product));
            }
            else
            {
                result.Add(new InventoryResponse
                {
                    Id = 0,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Category = product.Category,
                    QuantityOnHand = product.Quantity,
                    QuantityReserved = 0,
                    QuantityOnOrder = 0,
                    AvailableQuantity = product.Quantity,
                    ReorderLevel = 10,
                    ReorderQuantity = 20,
                    Status = product.Quantity <= 0 ? "OUT_OF_STOCK" : product.Quantity <= 10 ? "LOW" : "OK"
                });
            }
        }

        return result;
    }

    public async Task<InventoryResponse?> GetInventoryByProductIdAsync(int productId)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product == null) return null;

        var inventory = await _context.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory != null)
        {
            return MapToResponse(inventory, product);
        }

        return new InventoryResponse
        {
            Id = 0,
            ProductId = product.Id,
            ProductName = product.Name,
            Category = product.Category,
            QuantityOnHand = product.Quantity,
            QuantityReserved = 0,
            QuantityOnOrder = 0,
            AvailableQuantity = product.Quantity,
            ReorderLevel = 10,
            ReorderQuantity = 20,
            Status = product.Quantity <= 0 ? "OUT_OF_STOCK" : product.Quantity <= 10 ? "LOW" : "OK"
        };
    }

    public async Task<StockAlertResponse> GetLowStockAlertsAsync()
    {
        var allInventories = await GetAllInventoriesAsync();
        var response = new StockAlertResponse();

        foreach (var inv in allInventories)
        {
            if (inv.QuantityOnHand <= 0)
            {
                response.OutOfStockItems.Add(new StockAlertResponse.OutOfStockItem
                {
                    ProductId = inv.ProductId,
                    ProductName = inv.ProductName,
                    ReorderQuantity = inv.ReorderQuantity
                });
            }
            else if (inv.QuantityOnHand <= inv.ReorderLevel)
            {
                var urgency = inv.QuantityOnHand <= inv.ReorderLevel / 2 ? "high" : "medium";
                response.LowStockItems.Add(new StockAlertResponse.LowStockItem
                {
                    ProductId = inv.ProductId,
                    ProductName = inv.ProductName,
                    CurrentQuantity = inv.QuantityOnHand,
                    ReorderLevel = inv.ReorderLevel,
                    ReorderQuantity = inv.ReorderQuantity,
                    Urgency = urgency
                });
            }
        }

        response.TotalAlerts = response.LowStockItems.Count + response.OutOfStockItems.Count;
        return response;
    }

    public async Task<PaginatedStockMovementResponse> GetStockMovementsAsync(
        int? productId = null,
        string? movementType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.StockMovements.AsNoTracking().AsQueryable();

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        if (!string.IsNullOrEmpty(movementType) && Enum.TryParse<MovementType>(movementType, true, out var mt))
            query = query.Where(m => m.Type == mt);

        if (fromDate.HasValue)
            query = query.Where(m => m.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(m => m.CreatedAt <= toDate.Value);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var productIds = movements.Select(m => m.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        return new PaginatedStockMovementResponse
        {
            Data = movements.Select(m => new StockMovementResponse
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = products.GetValueOrDefault(m.ProductId, "Desconocido"),
                MovementType = m.Type.ToString(),
                Quantity = m.Quantity,
                BalanceBefore = m.BalanceBefore,
                BalanceAfter = m.BalanceAfter,
                RelatedDocumentId = m.RelatedDocumentId,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy
            }).ToList(),
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems
            }
        };
    }

    public async Task<int> GetAvailableQuantityAsync(int productId)
    {
        var inventory = await _context.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory != null)
            return inventory.GetAvailableQuantity();

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        return product?.Quantity ?? 0;
    }

    public async Task<StockMovementResponse> CreateStockMovementAsync(CreateStockMovementRequest request, string userId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);
        if (product == null)
            throw new InvalidOperationException($"Producto {request.ProductId} no encontrado");

        var balanceBefore = product.Quantity;
        var quantity = request.Quantity;

        if (!Enum.TryParse<MovementType>(request.MovementType, true, out var movementType))
            throw new InvalidOperationException($"Tipo de movimiento '{request.MovementType}' no válido");

        // Adjust quantity based on movement type
        if (movementType == MovementType.Sale || movementType == MovementType.Damage)
            product.Quantity -= quantity;
        else if (movementType == MovementType.Purchase || movementType == MovementType.Return)
            product.Quantity += quantity;

        product.UpdatedAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            Type = movementType,
            Quantity = quantity,
            BalanceBefore = balanceBefore,
            BalanceAfter = product.Quantity,
            RelatedDocumentId = request.RelatedDocumentId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        _logger.LogInformation("[MOVIMIENTO] Tipo={Type}, Producto={ProductId}, Cantidad={Qty}, Antes={Before}, Después={After}",
            movementType, request.ProductId, quantity, balanceBefore, product.Quantity);

        return new StockMovementResponse
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            ProductName = product.Name,
            MovementType = movement.Type.ToString(),
            Quantity = movement.Quantity,
            BalanceBefore = movement.BalanceBefore,
            BalanceAfter = movement.BalanceAfter,
            RelatedDocumentId = movement.RelatedDocumentId,
            Notes = movement.Notes,
            CreatedAt = movement.CreatedAt,
            CreatedBy = movement.CreatedBy
        };
    }

    public async Task<InventoryAdjustmentResponse> AdjustInventoryAsync(CreateInventoryAdjustmentRequest request, string userId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);
        if (product == null)
            throw new InvalidOperationException($"Producto {request.ProductId} no encontrado");

        var previousQuantity = product.Quantity;
        var adjustmentQuantity = request.NewQuantity - previousQuantity;

        product.Quantity = request.NewQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            Type = MovementType.Adjustment,
            Quantity = Math.Abs(adjustmentQuantity),
            BalanceBefore = previousQuantity,
            BalanceAfter = request.NewQuantity,
            AdjustmentReason = request.Reason,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        _logger.LogInformation("[AJUSTE] Producto={ProductId}, Anterior={Prev}, Nuevo={New}, Ajuste={Adj}",
            request.ProductId, previousQuantity, request.NewQuantity, adjustmentQuantity);

        return new InventoryAdjustmentResponse
        {
            Id = movement.Id,
            ProductId = request.ProductId,
            ProductName = product.Name,
            PreviousQuantity = previousQuantity,
            NewQuantity = request.NewQuantity,
            AdjustmentQuantity = adjustmentQuantity,
            Reason = request.Reason,
            Notes = request.Notes,
            CreatedAt = movement.CreatedAt,
            CreatedBy = userId
        };
    }

    public async Task<InventoryResponse?> UpdateReorderLevelsAsync(UpdateReorderLevelsRequest request, string userId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId);

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);

        if (product == null) return null;

        if (inventory == null)
        {
            inventory = new Inventory
            {
                ProductId = request.ProductId,
                QuantityOnHand = product.Quantity,
                QuantityReserved = 0,
                QuantityOnOrder = 0,
                ReorderLevel = request.ReorderLevel,
                ReorderQuantity = request.ReorderQuantity
            };
            _context.Inventories.Add(inventory);
        }
        else
        {
            inventory.ReorderLevel = request.ReorderLevel;
            inventory.ReorderQuantity = request.ReorderQuantity;
        }

        await _context.SaveChangesAsync();
        return MapToResponse(inventory, product);
    }

    public async Task<bool> InitializeInventoryAsync(int productId)
    {
        var exists = await _context.Inventories.AnyAsync(i => i.ProductId == productId);
        if (exists) return true;

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return false;

        _context.Inventories.Add(new Inventory
        {
            ProductId = productId,
            QuantityOnHand = product.Quantity,
            QuantityReserved = 0,
            QuantityOnOrder = 0,
            ReorderLevel = 10,
            ReorderQuantity = 20
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReserveStockAsync(int productId, int quantity)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory == null || inventory.GetAvailableQuantity() < quantity) return false;

        inventory.QuantityReserved += quantity;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReleaseReservedStockAsync(int productId, int quantity)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory == null || inventory.QuantityReserved < quantity) return false;

        inventory.QuantityReserved -= quantity;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasSufficientStockAsync(int productId, int quantity)
    {
        var inventory = await _context.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory != null)
            return inventory.GetAvailableQuantity() >= quantity;

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        return product != null && product.Quantity >= quantity;
    }

    private static InventoryResponse MapToResponse(Inventory inv, Product product)
    {
        var status = inv.QuantityOnHand <= 0 ? "OUT_OF_STOCK"
            : inv.QuantityOnHand <= inv.ReorderLevel ? "LOW"
            : "OK";

        return new InventoryResponse
        {
            Id = inv.Id,
            ProductId = inv.ProductId,
            ProductName = product.Name,
            Category = product.Category,
            QuantityOnHand = inv.QuantityOnHand,
            QuantityReserved = inv.QuantityReserved,
            QuantityOnOrder = inv.QuantityOnOrder,
            AvailableQuantity = inv.GetAvailableQuantity(),
            ReorderLevel = inv.ReorderLevel,
            ReorderQuantity = inv.ReorderQuantity,
            Status = status,
            LastCountedAt = inv.LastCountedAt,
            LastMovementAt = inv.LastMovementAt
        };
    }
}
