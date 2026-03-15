using BCrypt.Net;

namespace InventoryManagementAPI.Infrastructure.Security;

/// <summary>
/// Servicio de seguridad para hashear y verificar contraseñas con BCrypt
/// </summary>
/// <remarks>
/// Utiliza BCrypt con work factor 12 (factor de coste).
/// Work factor 12 proporciona un balance entre seguridad y performance.
/// Cada hash es único incluso para la misma contraseña (incluye salt automático).
/// </remarks>
public class PasswordHasher
{
    /// <summary>
    /// Hashea una contraseña usando BCrypt con work factor 12
    /// </summary>
    /// <param name="password">Contraseña en texto plano a hashear</param>
    /// <returns>Hash seguro de la contraseña (incluye salt)</returns>
    /// <remarks>
    /// El hash generado siempre es único gracias al salt aleatorio incluido.
    /// No es reversible (one-way encryption).
    /// </remarks>
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifica si una contraseña coincide con su hash
    /// </summary>
    /// <param name="password">Contraseña en texto plano a verificar</param>
    /// <param name="hash">Hash almacenado en base de datos</param>
    /// <returns>True si las contraseña coincide, false en caso contrario</returns>
    /// <remarks>
    /// Este método es seguro ante timing attacks.
    /// Siempre retorna false si el hash no es válido.
    /// </remarks>
    public static bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
