using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryManagementAPI.Application.Interfaces;

namespace InventoryManagementAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IInventoryNotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IInventoryNotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todos los productos con stock bajo (< 5 unidades)
    /// Solo accesible para administradores
    /// </summary>
    /// <returns>Lista de alertas de stock bajo</returns>
    /// <response code="200">Lista de alertas obtenida correctamente</response>
    /// <response code="401">No autenticado</response>
    /// <response code="403">No es administrador</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<object>> GetLowStockAlerts()
    {
        try
        {
            _logger.LogInformation("[NOTIFICATIONS] Solicitud de alertas de stock bajo");

            var alerts = await _notificationService.GetLowStockProductsAsync();

            return Ok(new
            {
                success = true,
                data = alerts,
                count = alerts.Count,
                message = $"Se encontraron {alerts.Count} productos con stock bajo",
                timestamp = DateTime.UtcNow.ToString("O")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Error al obtener alertas de stock bajo");
            return StatusCode(500, new
            {
                success = false,
                message = "Error al obtener alertas",
                timestamp = DateTime.UtcNow.ToString("O")
            });
        }
    }

    /// <summary>
    /// Endpoint de salud para verificar que el servicio de notificaciones está activo
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "InventoryNotifications",
            timestamp = DateTime.UtcNow.ToString("O")
        });
    }
}
