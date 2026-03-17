using Microsoft.AspNetCore.SignalR;

namespace InventoryManagementAPI.Api.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real de inventario
/// </summary>
public class InventoryNotificationHub : Hub
{
    private readonly ILogger<InventoryNotificationHub> _logger;

    public InventoryNotificationHub(ILogger<InventoryNotificationHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se conecta al hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"[SIGNALR] Cliente conectado: {connectionId}");
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se desconecta del hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"[SIGNALR] Cliente desconectado: {connectionId}");
        
        if (exception != null)
        {
            _logger.LogError(exception, $"[SIGNALR] Error en desconexión: {connectionId}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Registra un cliente para recibir notificaciones (opcional, para filtrar por rol)
    /// </summary>
    public async Task RegisterForNotifications(string userRole)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"[SIGNALR] Usuario registrado para notificaciones - ConnectionId: {connectionId}, Rol: {userRole}");
        
        // Agregar el cliente a un grupo según su rol
        if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            await Groups.AddToGroupAsync(connectionId, "Administrators");
            _logger.LogInformation($"[SIGNALR] Cliente {connectionId} añadido al grupo 'Administrators'");
        }
        else if (userRole.Equals("Employee", StringComparison.OrdinalIgnoreCase))
        {
            await Groups.AddToGroupAsync(connectionId, "Employees");
            _logger.LogInformation($"[SIGNALR] Cliente {connectionId} añadido al grupo 'Employees'");
        }
        
        await Clients.Caller.SendAsync("RegistrationSuccess", new { message = "Notificaciones activadas", timestamp = DateTime.UtcNow.ToString("O") });
    }

    /// <summary>
    /// Desregistra un cliente (opcional, para limpiar grupos)
    /// </summary>
    public async Task UnregisterFromNotifications()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"[SIGNALR] Usuario desregistrado - ConnectionId: {connectionId}");
        
        await Groups.RemoveFromGroupAsync(connectionId, "Administrators");
        await Groups.RemoveFromGroupAsync(connectionId, "Employees");
        
        await Clients.Caller.SendAsync("UnregistrationSuccess", new { message = "Notificaciones desactivadas", timestamp = DateTime.UtcNow.ToString("O") });
    }

    /// <summary>
    /// Método interno para enviar notificación de stock bajo (llamado desde backend)
    /// </summary>
    public async Task NotifyLowStockToAdmins(object notification)
    {
        _logger.LogWarning($"[NOTIFICATION] Notificación de stock bajo enviada a administradores");
        
        // Envía solo a los administradores
        await Clients.Group("Administrators").SendAsync("LowStockAlert", notification);
    }
}
