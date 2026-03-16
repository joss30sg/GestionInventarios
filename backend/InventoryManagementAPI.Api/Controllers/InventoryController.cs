using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using InventoryManagementAPI.Application.DTOs.Inventory;
using InventoryManagementAPI.Application.Interfaces;
using System.Security.Claims;

namespace InventoryManagementAPI.Api.Controllers;

/// <summary>
/// API RESTful para gestión de inventario y stock
/// Monitorea movimientos de stock, reservas y alertas de reorden
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IValidator<CreateStockMovementRequest> _movementValidator;
    private readonly IValidator<CreateInventoryAdjustmentRequest> _adjustmentValidator;
    private readonly IValidator<UpdateReorderLevelsRequest> _reorderValidator;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryService inventoryService,
        IValidator<CreateStockMovementRequest> movementValidator,
        IValidator<CreateInventoryAdjustmentRequest> adjustmentValidator,
        IValidator<UpdateReorderLevelsRequest> reorderValidator,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _movementValidator = movementValidator;
        _adjustmentValidator = adjustmentValidator;
        _reorderValidator = reorderValidator;
        _logger = logger;
    }

    // ==================== CONSULTAS ====================

    [HttpGet]
    public async Task<ActionResult<List<InventoryResponse>>> GetAll()
    {
        try
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync();
            return Ok(new { success = true, data = inventories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inventarios");
            return StatusCode(500, new { success = false, message = "Error al obtener inventarios" });
        }
    }

    /// <summary>
    /// Obtiene inventario de un producto
    /// </summary>
    /// <remarks>
    /// Retorna información detallada de stock:
    /// - Stock disponible (OnHand - Reserved)
    /// - Stock reservado por pedidos
    /// - Stock en tránsito
    /// - Estado general (OK, LOW, OUT_OF_STOCK)
    /// </remarks>
    /// <response code="200">Inventario obtenido</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("products/{productId}")]
    public async Task<ActionResult<InventoryResponse>> GetInventoryByProductId(int productId)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);

            if (inventory == null)
            {
                _logger.LogWarning($"Inventario no encontrado para producto {productId}");
                return NotFound(new { success = false, message = "Producto no encontrado" });
            }

            return Ok(new { success = true, data = inventory });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inventario");
            return StatusCode(500, new { success = false, message = "Error al obtener inventario" });
        }
    }

    /// <summary>
    /// Obtiene disponibilidad (cantidad disponible para vender) de un producto
    /// </summary>
    /// <remarks>
    /// Cálculo: QuantityOnHand - QuantityReserved
    /// Retorna solo el número disponible
    /// </remarks>
    [HttpGet("products/{productId}/available")]
    public async Task<ActionResult<int>> GetAvailableQuantity(int productId)
    {
        try
        {
            var available = await _inventoryService.GetAvailableQuantityAsync(productId);
            return Ok(new { success = true, available });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cantidad disponible");
            return StatusCode(500, new { success = false, message = "Error al obtener disponibilidad" });
        }
    }

    /// <summary>
    /// Obtiene alertas de stock bajo
    /// </summary>
    /// <remarks>
    /// Retorna productos con:
    /// - Stock debajo del nivel de reorden
    /// - Productos sin stock
    /// 
    /// Autorización: Admin y Employee
    /// </remarks>
    /// <response code="200">Alertas obtenidas</response>
    /// <response code="403">Sin permisos</response>
    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<StockAlertResponse>> GetLowStockAlerts()
    {
        try
        {
            _logger.LogInformation("Obteniendo alertas de stock bajo");
            var alerts = await _inventoryService.GetLowStockAlertsAsync();

            return Ok(new { success = true, data = alerts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alertas");
            return StatusCode(500, new { success = false, message = "Error al obtener alertas" });
        }
    }

    /// <summary>
    /// Obtiene historial de movimientos de inventario con filtros
    /// </summary>
    /// <remarks>
    /// Filtros disponibles:
    /// - productId: ID del producto (opcional)
    /// - movementType: Tipo (purchase, sale, adjustment, return, damage, transfer)
    /// - fromDate: Fecha inicio (ISO 8601)
    /// - toDate: Fecha fin (ISO 8601)
    /// - page: Número de página (defecto: 1)
    /// - pageSize: Registros por página (defecto: 20)
    /// 
    /// Autorización: Solo Admin
    /// 
    /// Ejemplo: GET /api/v1/inventory/movements?productId=123&from=2026-01-01&to=2026-03-18
    /// </remarks>
    [HttpGet("movements")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaginatedStockMovementResponse>> GetStockMovements(
        [FromQuery] int? productId = null,
        [FromQuery] string? movementType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            _logger.LogInformation($"Obteniendo movimientos: producto={productId}, tipo={movementType}, página={page}");

            var movements = await _inventoryService.GetStockMovementsAsync(productId, movementType, fromDate, toDate, page, pageSize);

            return Ok(new { success = true, data = movements });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos");
            return StatusCode(500, new { success = false, message = "Error al obtener movimientos" });
        }
    }

    // ==================== MOVIMIENTOS DE STOCK ====================

    /// <summary>
    /// Registra un movimiento de inventario
    /// </summary>
    /// <remarks>
    /// Tipos de movimiento:
    /// - purchase: Compra a proveedor
    /// - sale: Venta a cliente
    /// - adjustment: Ajuste manual
    /// - return: Devolución de cliente
    /// - damage: Daño/pérdida
    /// - transfer: Transferencia (si aplica)
    /// 
    /// Autorización: Solo Admin
    /// 
    /// Ejemplo:
    /// ```json
    /// {
    ///   "productId": 123,
    ///   "movementType": "purchase",
    ///   "quantity": 50,
    ///   "relatedDocumentId": 456,
    ///   "notes": "Compra a distribuidor ABC"
    /// }
    /// ```
    /// </remarks>
    /// <response code="201">Movimiento registrado exitosamente</response>
    /// <response code="400">Validación fallida o cantidad inválida</response>
    [HttpPost("movements")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StockMovementResponse>> CreateStockMovement([FromBody] CreateStockMovementRequest request)
    {
        try
        {
            var validationResult = await _movementValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validación fallida",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            var userId = GetUserIdFromToken();
            var movement = await _inventoryService.CreateStockMovementAsync(request, userId);

            _logger.LogInformation($"Movimiento registrado: {movement.Id}, producto {request.ProductId}, cantidad {request.Quantity}");

            return CreatedAtAction(nameof(GetStockMovements), new { movement.Id }, new
            {
                success = true,
                message = "Movimiento registrado exitosamente",
                data = movement
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar movimiento");
            return StatusCode(500, new { success = false, message = "Error al registrar movimiento" });
        }
    }

    // ==================== AJUSTES ====================

    /// <summary>
    /// Registra un ajuste de inventario (conteo físico)
    /// </summary>
    /// <remarks>
    /// Se usa cuando se realiza un conteo físico del inventario
    /// y hay discrepancias con el sistema.
    /// 
    /// El sistema calcula la diferencia automáticamente:
    /// Ajuste = NewQuantity - QuantityOnHand
    /// 
    /// Autorización: Solo Admin
    /// 
    /// Ejemplo:
    /// ```json
    /// {
    ///   "productId": 123,
    ///   "newQuantity": 45,
    ///   "reason": "Conteo físico trimestral",
    ///   "notes": "Se encontraron 5 unidades menos que en el sistema"
    /// }
    /// ```
    /// </remarks>
    /// <response code="201">Ajuste registrado</response>
    /// <response code="400">Validación fallida</response>
    [HttpPost("adjustments")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InventoryAdjustmentResponse>> AdjustInventory([FromBody] CreateInventoryAdjustmentRequest request)
    {
        try
        {
            var validationResult = await _adjustmentValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validación fallida",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            var userId = GetUserIdFromToken();
            var adjustment = await _inventoryService.AdjustInventoryAsync(request, userId);

            _logger.LogInformation($"Ajuste registrado: {adjustment.Id}, producto {request.ProductId}, ajuste {adjustment.AdjustmentQuantity}");

            return CreatedAtAction(nameof(GetInventoryByProductId), new { productId = adjustment.ProductId }, new
            {
                success = true,
                message = "Ajuste registrado exitosamente",
                data = adjustment
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ajustar inventario");
            return StatusCode(500, new { success = false, message = "Error al ajustar inventario" });
        }
    }

    /// <summary>
    /// Actualiza niveles de reorden de un producto
    /// </summary>
    /// <remarks>
    /// Establece:
    /// - ReorderLevel: Cantidad mínima antes de alerta
    /// - ReorderQuantity: Cantidad a comprar cuando se alcanza mínimo
    /// 
    /// Autorización: Solo Admin
    /// </remarks>
    [HttpPatch("products/{productId}/reorder-levels")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InventoryResponse>> UpdateReorderLevels(int productId, [FromBody] UpdateReorderLevelsRequest request)
    {
        try
        {
            request.ProductId = productId;

            var validationResult = await _reorderValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validación fallida",
                    errors = validationResult.Errors
                });
            }

            var userId = GetUserIdFromToken();
            var inventory = await _inventoryService.UpdateReorderLevelsAsync(request, userId);

            if (inventory == null)
            {
                return NotFound(new { success = false, message = "Producto no encontrado" });
            }

            _logger.LogInformation($"Niveles de reorden actualizados para producto {productId}");

            return Ok(new
            {
                success = true,
                message = "Niveles de reorden actualizados",
                data = inventory
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar niveles de reorden");
            return StatusCode(500, new { success = false, message = "Error al actualizar" });
        }
    }

    private string GetUserIdFromToken()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
    }
}
