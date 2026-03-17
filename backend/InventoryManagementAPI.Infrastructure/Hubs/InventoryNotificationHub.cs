using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace InventoryManagementAPI.Infrastructure.Hubs;

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

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("[SIGNALR] Cliente conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("[SIGNALR] Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        if (exception != null)
        {
            _logger.LogError(exception, "[SIGNALR] Error en desconexión: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Registra un cliente en su grupo de rol para recibir notificaciones filtradas
    /// </summary>
    public async Task RegisterForNotifications(string userRole)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("[SIGNALR] Usuario registrado - ConnectionId: {ConnectionId}, Rol: {Role}", connectionId, userRole);

        if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            await Groups.AddToGroupAsync(connectionId, "Administrators");
        }
        else if (userRole.Equals("Employee", StringComparison.OrdinalIgnoreCase))
        {
            await Groups.AddToGroupAsync(connectionId, "Employees");
        }

        await Clients.Caller.SendAsync("RegistrationSuccess", new { message = "Notificaciones activadas", timestamp = DateTime.UtcNow.ToString("O") });
    }

    /// <summary>
    /// Desregistra un cliente de los grupos de notificaciones
    /// </summary>
    public async Task UnregisterFromNotifications()
    {
        var connectionId = Context.ConnectionId;
        await Groups.RemoveFromGroupAsync(connectionId, "Administrators");
        await Groups.RemoveFromGroupAsync(connectionId, "Employees");
        await Clients.Caller.SendAsync("UnregistrationSuccess", new { message = "Notificaciones desactivadas", timestamp = DateTime.UtcNow.ToString("O") });
    }
}
