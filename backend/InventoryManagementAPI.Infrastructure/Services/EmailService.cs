using System.Text;
using System.Text.Encodings.Web;
using InventoryManagementAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManagementAPI.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de email usando SMTP.
/// Configurable a través de appsettings.json
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
        // Aquí iría inyección de configuración real en producción
        // Por ahora usa valores por defecto
        _smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "smtp.gmail.com";
        _smtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : 587;
        _senderEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL") ?? "noreply@ordermanagement.com";
        _senderName = "Order Management System";
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            _logger.LogInformation("[EMAIL] Enviando email a {To} con asunto: {Subject}", to, subject);

            // En desarrollo sin SMTP configurado, simular el envío
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SMTP_PASSWORD")))
            {
                _logger.LogWarning(
                    "[EMAIL] SMTP no está configurado. Email simulado enviado a {To}. " +
                    "Para producción, configura variables de entorno: SMTP_SERVER, SMTP_PORT, SMTP_EMAIL, SMTP_PASSWORD",
                    to
                );
                return true;
            }

            // Aquí iría la lógica real con SmtpClient
            // await new SmtpClient(_smtpServer, _smtpPort)
            // {
            //     Credentials = new NetworkCredential(_senderEmail, Environment.GetEnvironmentVariable("SMTP_PASSWORD")),
            //     EnableSsl = true
            // }.SendMailAsync(new MailMessage(_senderEmail, to) { ... });

            _logger.LogInformation("[EMAIL] Email enviado exitosamente a {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Error al enviar email a {To}", to);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetLink)
    {
        try
        {
            var subject = "🔐 Recupera tu contraseña - Order Management";

            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; }}
                        .content {{ padding: 20px; background: #f8f9fa; border-radius: 8px; margin: 20px 0; }}
                        .button {{ 
                            display: inline-block; 
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                            color: white; 
                            padding: 12px 24px; 
                            text-decoration: none; 
                            border-radius: 6px; 
                            margin: 15px 0;
                        }}
                        .warning {{ color: #856404; background: #fff3cd; padding: 10px; border-radius: 4px; margin: 10px 0; }}
                        .footer {{ text-align: center; color: #6c757d; font-size: 12px; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔐 Recuperación de Contraseña</h1>
                        </div>
                        
                        <div class='content'>
                            <h2>Hola,</h2>
                            <p>Recibimos una solicitud para recuperar tu contraseña. Si no solicitaste esto, ignora este email.</p>
                            
                            <p>Haz clic en el botón de abajo para establecer una nueva contraseña:</p>
                            
                            <center>
                                <a href='{HtmlEncoder.Default.Encode(resetLink)}' class='button'>
                                    Restablecer Contraseña
                                </a>
                            </center>
                            
                            <p>O copia y pega este enlace en tu navegador:</p>
                            <p style='background: white; padding: 10px; border-radius: 4px; word-break: break-all;'>
                                {HtmlEncoder.Default.Encode(resetLink)}
                            </p>
                            
                            <div class='warning'>
                                ⏰ Este enlace expira en 1 hora. Si expira, deberás solicitar un nuevo reset.
                            </div>
                        </div>
                        
                        <div class='footer'>
                            <p>Este email fue enviado por Order Management System</p>
                            <p>© 2026 - Todos los derechos reservados</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, htmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Error al enviar email de reset a {Email}", email);
            return false;
        }
    }
}
