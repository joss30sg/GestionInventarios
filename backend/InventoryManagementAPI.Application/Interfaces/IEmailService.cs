using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Application.Interfaces;

/// <summary>
/// Interfaz para enviar emails desde la aplicación.
/// Implementa un proveedor SMTP configurable.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un email simple con asunto y cuerpo HTML.
    /// </summary>
    /// <param name="to">Dirección de correo destinatario</param>
    /// <param name="subject">Asunto del email</param>
    /// <param name="htmlBody">Cuerpo del email en formato HTML</param>
    /// <returns>true si se envió exitosamente, false si hubo error</returns>
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody);

    /// <summary>
    /// Envía un email de recuperación de contraseña con token y enlace.
    /// </summary>
    /// <param name="email">Correo del usuario</param>
    /// <param name="resetToken">Token de reset codificado en Base64</param>
    /// <param name="resetLink">Enlace completo para resetear contraseña (ej: https://tuapp.com/reset?token=...)</param>
    /// <returns>true si se envió exitosamente</returns>
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetLink);
}
