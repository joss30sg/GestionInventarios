using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Application.DTOs.Auth;

/// <summary>
/// Solicitud para registrar un nuevo usuario en el sistema.
/// Requiere nombre de usuario único, email válido y contraseña segura.
/// </summary>
public class UserRegisterRequest
{
    /// <summary>Nombre de usuario único (3-50 caracteres alfanuméricos)</summary>
    /// <example>juan.perez</example>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El nombre debe contener solo letras, números, puntos, guiones y guiones bajos")]
    [Display(Description = "Identificador único del usuario (alfanumérico, sin espacios)")]
    public string Username { get; set; } = string.Empty;

    /// <summary>Dirección de correo electrónico válida y única</summary>
    /// <example>juan.perez@empresa.com</example>
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
    [Display(Description = "Email válido para notificaciones y recuperación de contraseña")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Contraseña segura (mínimo 8 caracteres, 1 mayúscula, 1 minúscula, 1 número, 1 especial)</summary>
    /// <example>SecurePass123!</example>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    [DataType(DataType.Password)]
    [Display(Description = "Contraseña segura (mín 8 caracteres, mayúscula, minúscula, número, carácter especial)")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Rol del usuario (opcional, default: Employee)</summary>
    /// <example>Employee</example>
    public string? Role { get; set; }
}

/// <summary>
/// Solicitud para autenticar un usuario existente.
/// Valida credenciales y retorna token JWT si es exitoso.
/// </summary>
public class UserLoginRequest
{
    /// <summary>Nombre de usuario registrado</summary>
    /// <example>juan.perez</example>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [Display(Description = "Nombre de usuario registrado en el sistema")]
    public string Username { get; set; } = string.Empty;

    /// <summary>Contraseña del usuario</summary>
    /// <example>SecurePass123!</example>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [DataType(DataType.Password)]
    [Display(Description = "Contraseña asociada a la cuenta")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta que contiene información del usuario autenticado.
/// Se retorna junto con el token JWT después del login exitoso.
/// </summary>
public class UserResponse
{
    /// <summary>Identificador único del usuario</summary>
    /// <example>42</example>
    [Display(Description = "ID único del usuario en el sistema")]
    public int Id { get; set; }

    /// <summary>Nombre de usuario (único en el sistema)</summary>
    /// <example>juan.perez</example>
    [Display(Description = "Nombre de usuario único")]
    public string Username { get; set; } = string.Empty;

    /// <summary>Dirección de correo electrónico del usuario</summary>
    /// <example>juan.perez@empresa.com</example>
    [Display(Description = "Email del usuario")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Rol del usuario en el sistema (typically "User")</summary>
    /// <example>User</example>
    [Display(Description = "Rol del usuario (User, Admin, etc)")]
    public string Role { get; set; } = "User";

    /// <summary>Estado de actividad de la cuenta</summary>
    /// <example>true</example>
    [Display(Description = "Indica si la cuenta está activa")]
    public bool IsActive { get; set; }

    /// <summary>Fecha/hora de creación de la cuenta (UTC, ISO 8601)</summary>
    /// <example>2026-02-23T10:15:30.000Z</example>
    [Display(Description = "Fecha de creación de la cuenta en UTC")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Respuesta estándar para operaciones de autenticación (registro y login).
/// Incluye token JWT, información del usuario y estado de la operación.
/// </summary>
public class AuthResponse
{
    /// <summary>Indica si la operación fue exitosa</summary>
    /// <example>true</example>
    [Display(Description = "true si autenticación exitosa, false si falló")]
    public bool Success { get; set; }

    /// <summary>Mensaje descriptivo del resultado (éxito o error)</summary>
    /// <example>Autenticación exitosa</example>
    [Display(Description = "Mensaje descriptivo de la respuesta")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Token JWT Bearer para autorización (null si hubo error)</summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0MiIsImlhdCI6MTcwODY2NjEzMH0.x7gV8j9kL2m5nQ3r4s6t7u8v9w0x1y2z</example>
    [Display(Description = "Token JWT para usar en Authorization Bearer header")]
    public string? Token { get; set; }

    /// <summary>Información del usuario autenticado (null si hubo error)</summary>
    [Display(Description = "Datos del usuario autenticado")]
    public UserResponse? User { get; set; }
}

/// <summary>
/// Solicitud para recuperar contraseña olvidada.
/// Se envía un enlace de reset al correo electrónico del usuario.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>Dirección de correo electrónico registrada</summary>
    /// <example>juan.perez@empresa.com</example>
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
    [Display(Description = "Email registrado para recuperar acceso")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta para solicitud de recuperación de contraseña.
/// Confirma que se envió el enlace al correo (sin revelar si existe la cuenta).
/// </summary>
public class ForgotPasswordResponse
{
    /// <summary>Indica si la solicitud fue procesada exitosamente</summary>
    /// <example>true</example>
    [Display(Description = "true si se procesó la solicitud")]
    public bool Success { get; set; }

    /// <summary>Mensaje público (no revela si el email existe)</summary>
    /// <example>Si existe una cuenta con ese correo, recibirás instrucciones de reset</example>
    [Display(Description = "Mensaje seguro sobre el resultado")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Solicitud para resetear contraseña usando un token válido.
/// El token se envía por email y tiene expiración limitada.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>Token de reset recibido por email</summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    [Required(ErrorMessage = "El token es requerido")]
    [Display(Description = "Token de reset recibido en el email")]
    public string Token { get; set; } = string.Empty;

    /// <summary>Nueva contraseña segura (mismos requisitos que registro)</summary>
    /// <example>NewSecurePass123!</example>
    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    [DataType(DataType.Password)]
    [Display(Description = "Nueva contraseña (mín 8 caracteres, mayúscula, minúscula, número, carácter especial)")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>Confirmación de la nueva contraseña</summary>
    /// <example>NewSecurePass123!</example>
    [Required(ErrorMessage = "La confirmación es requerida")]
    [DataType(DataType.Password)]
    [Display(Description = "Debe coincidir exactamente con la nueva contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta para solicitud de reset de contraseña.
/// Confirma si el reset fue exitoso.
/// </summary>
public class ResetPasswordResponse
{
    /// <summary>Indica si el reset fue exitoso</summary>
    /// <example>true</example>
    [Display(Description = "true si la contraseña fue cambiada exitosamente")]
    public bool Success { get; set; }

    /// <summary>Mensaje descriptivo del resultado</summary>
    /// <example>Contraseña restablecida exitosamente. Inicia sesión con tu nueva contraseña.</example>
    [Display(Description = "Mensaje sobre el resultado del reset")]
    public string Message { get; set; } = string.Empty;
}
