using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using InventoryManagementAPI.Application.Interfaces;

namespace InventoryManagementAPI.Infrastructure.Security;

/// <summary>
/// Servicio para generar y validar JWT Bearer Tokens
/// </summary>
/// <remarks>
/// Genera tokens firmados con HS256 (HMAC-SHA256).
/// Los tokens incluyen claims para identificar y autorizar usuarios.
/// Soporta validación de firma, issuer, audience y tiempo de expiración.
/// </remarks>
public class JwtTokenService : IJwtTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    /// <summary>
    /// Inicializa el servicio JWT con configuración
    /// </summary>
    /// <param name="secretKey">Clave secreta para firmar tokens (mínimo 32 caracteres)</param>
    /// <param name="issuer">Emisor del token (nombre de la aplicación)</param>
    /// <param name="audience">Audience del token (cliente que lo consumirá)</param>
    /// <param name="expirationMinutes">Minutos hasta que el token expire (defecto 60)</param>
    public JwtTokenService(string secretKey, string issuer, string audience, int expirationMinutes = 60)
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
        _expirationMinutes = expirationMinutes;
    }

    /// <summary>
    /// Genera un JWT Bearer Token firmado
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="role">Rol del usuario (Employee, Admin)</param>
    /// <returns>Token JWT firmado con HS256</returns>
    /// <remarks>
    /// El token contiene los siguientes claims:
    /// - ClaimTypes.NameIdentifier: UserId (para identificar al usuario)
    /// - ClaimTypes.Name: Username
    /// - ClaimTypes.Role: Rol para autorización basada en roles
    /// - UserId: Custom claim adicional para compatibilidad
    /// - Role: Custom claim adicional para compatibilidad
    /// 
    /// El token expira después de ExpirationMinutes (configurable, defecto 60).
    /// </remarks>
    public string GenerateToken(int userId, string username, string role = "User")
    {
        // Crear clave de seguridad a partir de la secret key
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Agregar claims al token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),  // Para obtener UserId desde HttpContext.User
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),  // Para autorización con [Authorize(Roles = "Admin")]
            new("UserId", userId.ToString()),  // Claim personalizado adicional
            new("Role", role)  // Claim personalizado adicional
        };

        // Crear el JWT con todos los parámetros de seguridad
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Valida un JWT token y verifica su integridad
    /// </summary>
    /// <param name="token">Token JWT a validar</param>
    /// <returns>True si el token es válido, false en caso contrario</returns>
    /// <remarks>
    /// Valida:
    /// - Firma (HS256)
    /// - Issuer
    /// - Audience
    /// - Tiempo de expiración (ClockSkew = 0, sin tolerancia de tiempo)
    /// - Formato y estructura
    /// 
    /// Retorna false sin lanzar excepciones si el token es inválido.
    /// </remarks>
    public bool ValidateToken(string token)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            // Validar token con parámetros de seguridad estrictos
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,  // Validar firma HS256
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,  // Validar expiración
                ClockSkew = TimeSpan.Zero  // Sin tolerancia de tiempo
            }, out SecurityToken validatedToken);

            return validatedToken is JwtSecurityToken;
        }
        catch
        {
            return false;
        }
    }
}
