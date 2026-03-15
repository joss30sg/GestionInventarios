using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using InventoryManagementAPI.Application.DTOs.Auth;
using InventoryManagementAPI.Application.Interfaces;
using System.Security.Claims;

namespace InventoryManagementAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<UserLoginRequest> _loginValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IJwtTokenService jwtTokenService,
        IValidator<UserLoginRequest> loginValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="request">Datos de registro (username, email, password, role)</param>
    /// <returns>JWT Token y datos del usuario registrado</returns>
    /// <response code="200">Registro exitoso. Retorna token JWT válido por 60 minutos.</response>
    /// <response code="400">Solicitud mal formada o usuario ya existe.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] UserRegisterRequest request)
    {
        try
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new 
                { 
                    success = false,
                    message = "Username y password son requeridos"
                });
            }

            // Registrar usuario
            var result = await _authService.RegisterAsync(request);

            // Verificar si el registro fue exitoso
            if (!result.Success)
            {
                _logger.LogWarning($"Registro fallido: {result.Message}");
                return BadRequest(new 
                { 
                    success = false,
                    message = result.Message
                });
            }

            // Log de auditoría
            _logger.LogInformation($"[AUDIT] Registro exitoso: Usuario={request.Username} | Rol={request.Role ?? "Employee"} | Timestamp={DateTime.UtcNow:O}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Excepción en registro: {ex.Message}");
            return StatusCode(500, new 
            { 
                success = false,
                message = "Error al registrar usuario",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Autentica un usuario y genera JWT Bear Token
    /// </summary>
    /// <param name="request">Credenciales (username, password)</param>
    /// <returns>JWT Token y datos del usuario autenticado</returns>
    /// <response code="200">Autenticación exitosa. Retorna token JWT válido por 60 minutos.</response>
    /// <response code="400">Solicitud mal formada.</response>
    /// <response code="401">Credenciales inválidas o usuario inactivo.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] UserLoginRequest request)
    {
        try
        {
            // Validar modelo de entrada
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning($"Validación fallida en login para usuario: {request.Username}");
                return BadRequest(new 
                { 
                    success = false,
                    message = "Datos de login inválidos",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            // Procesar autenticación
            var result = await _authService.LoginAsync(request);

            // Retornar 401 si las credenciales no son válidas
            if (!result.Success)
            {
                _logger.LogWarning($"[SECURITY] Intento de login fallido para usuario: {request.Username} | Motivo: {result.Message}");
                return Unauthorized(new 
                { 
                    success = false,
                    message = result.Message
                });
            }

            // Log de auditoría para login exitoso
            _logger.LogInformation($"[AUDIT] Login exitoso: Usuario={request.Username} | Rol={result.User?.Role} | IP={HttpContext.Connection.RemoteIpAddress} | Timestamp={DateTime.UtcNow:O}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Excepción en login: {ex.Message}");
            return StatusCode(500, new 
            { 
                success = false,
                message = "Error interno del servidor",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado
    /// </summary>
    /// <remarks>
    /// Requiere JWT Bearer Token válido en el header Authorization.
    /// El token se valida automáticamente por el middleware [Authorize].
    /// </remarks>
    /// <returns>Datos del usuario autenticado (id, username, email, role)</returns>
    /// <response code="200">Perfil obtenido exitosamente.</response>
    /// <response code="401">Token no válido, expirado o inexistente.</response>
    /// <response code="404">Usuario no encontrado en base de datos.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        try
        {
            // Extraer UserId del JWT token (claim agregado durante login)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Validar que el claim existe y es convertible a int
            if (!int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("[SECURITY] Intento de acceso con token malformado");
                return Unauthorized(new 
                { 
                    success = false,
                    message = "Token inválido o malformado"
                });
            }

            // Obtener perfil del usuario
            var user = await _authService.GetUserByIdAsync(userId);
            
            // Usuario no encontrado (puede ocurrir si fue eliminado después de login)
            if (user == null)
            {
                _logger.LogWarning($"[SECURITY] Perfil no encontrado para UserId: {userId}");
                return NotFound(new 
                { 
                    success = false,
                    message = "Usuario no encontrado"
                });
            }

            _logger.LogInformation($"[AUDIT] Perfil obtenido: Usuario={user.Username} | Timestamp={DateTime.UtcNow:O}");
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Excepción al obtener perfil: {ex.Message}");
            return StatusCode(500, new 
            { 
                success = false,
                message = "Error interno del servidor",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Genera un JWT Token de demostración para testing  
    /// </summary>
    /// <remarks>
    /// Este endpoint está diseñado solo para testing en Swagger.
    /// Genera un token válido con ID de usuario demo para que puedas probar los endpoints protegidos.
    /// 
    /// Instrucciones:
    /// 1. Ejecuta este endpoint (GET /api/auth/demo-token)
    /// 2. Copia el valor de "token" de la respuesta JSON
    /// 3. Haz clic en el botón "Authorize" (arriba a la derecha)
    /// 4. Pega el token en el campo (sin comillas)
    /// 5. Haz clic en "Authorize"
    /// 6. Ahora puedes probar todos los endpoints protegidos (/api/orders/*)
    /// </remarks>
    /// <returns>JWT Token válido para testing</returns>
    /// <response code="200">Token de demostración generado exitosamente.</response>
    [HttpGet("demo-token")]
    public IActionResult GetDemoToken()
    {
        try
        {
            // Generar token de demostración para usuario ID 1 con rol Admin
            var demoToken = _jwtTokenService.GenerateToken(
                userId: 1,
                username: "demo_user",
                role: "Admin"
            );

            _logger.LogInformation("[DEMO] Token de demostración generado para testing en Swagger");

            return Ok(new 
            { 
                success = true,
                token = demoToken,
                expiresIn = 3600,
                instructions = "Copia el token anterior e introdúcelo en el botón Authorize (arriba a la derecha)",
                message = "Token de demostración válido por 60 minutos."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al generar token de demo: {ex.Message}");
            return StatusCode(500, new 
            { 
                success = false,
                message = "Error al generar token de demostración",
                timestamp = DateTime.UtcNow
            });
        }
    }

}
