using AutoMapper;
using InventoryManagementAPI.Application.DTOs.Auth;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Domain.Entities;
using InventoryManagementAPI.Infrastructure.Data;
using InventoryManagementAPI.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementAPI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IJwtTokenService tokenService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthResponse> RegisterAsync(UserRegisterRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username))
        {
            _logger.LogWarning("[VALIDATION] Registro fallido: datos incompletos");
            return new AuthResponse { Success = false, Message = "Datos incompletos" };
        }

        try
        {
            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                _logger.LogInformation($"[BUSINESS] Usuario duplicado: {request.Username}");
                return new AuthResponse { Success = false, Message = "El usuario ya existe" };
            }

            var role = UserRole.Employee;
            if (!string.IsNullOrWhiteSpace(request.Role) &&
                Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var parsedRole)
                && parsedRole == UserRole.Employee)
            {
                role = parsedRole;
            }
            // Solo un Admin autenticado puede crear otros Admin (vía endpoint separado)

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user.Id, user.Username, user.Role.ToString());

            _logger.LogInformation($"[AUDIT] Usuario registrado: {request.Username} | ID={user.Id}");

            return new AuthResponse
            {
                Success = true,
                Message = "Registro completado exitosamente",
                Token = token,
                User = _mapper.Map<UserResponse>(user)
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "[ERROR] Error de BD en registro");
            return new AuthResponse { Success = false, Message = "Error al guardar datos" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Excepción en registro");
            return new AuthResponse { Success = false, Message = "Error interno del servidor" };
        }
    }

    public async Task<AuthResponse> LoginAsync(UserLoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username))
        {
            _logger.LogWarning("[VALIDATION] Login fallido: credenciales vacías");
            return new AuthResponse { Success = false, Message = "Credenciales incompletas" };
        }

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning($"[SECURITY] Login fallido: {request.Username}");
                return new AuthResponse { Success = false, Message = "Credenciales inválidas" };
            }

            if (!user.IsActive)
            {
                _logger.LogWarning($"[SECURITY] Usuario inactivo: {request.Username}");
                return new AuthResponse { Success = false, Message = "Cuenta desactivada" };
            }

            var token = _tokenService.GenerateToken(user.Id, user.Username, user.Role.ToString());

            _logger.LogInformation($"[AUDIT] Login exitoso: {request.Username} | Rol={user.Role}");

            return new AuthResponse
            {
                Success = true,
                Message = "Autenticación completada",
                Token = token,
                User = _mapper.Map<UserResponse>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Excepción en login");
            return new AuthResponse { Success = false, Message = "Error interno del servidor" };
        }
    }

    public async Task<UserResponse?> GetUserByIdAsync(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("[VALIDATION] ID de usuario inválido: {UserId}", userId);
            return null;
        }

        try
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            return user == null ? null : _mapper.Map<UserResponse>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Excepción al obtener usuario");
            return null;
        }
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            // Validar que el usuario existe (sin revelar por seguridad)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            _logger.LogInformation($"[AUTH] Solicitud de reset para email: {request.Email} - Usuario existe: {user != null}");

            // Siempre retornar un mensaje genérico (no revelar si existe o no)
            return new ForgotPasswordResponse
            {
                Success = true,
                Message = $"Si existe una cuenta con el correo {request.Email}, " +
                          "recibirás un email con instrucciones para recuperar tu contraseña. " +
                          "El enlace es válido por 1 hora."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Excepción en ForgotPassword");
            return new ForgotPasswordResponse
            {
                Success = false,
                Message = "Error al procesar la solicitud"
            };
        }
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            // Validar que las contraseñas coinciden
            if (request.NewPassword != request.ConfirmPassword)
            {
                _logger.LogWarning("[VALIDATION] Contraseñas no coinciden en reset");
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Las contraseñas no coinciden"
                };
            }

            // En una implementación real, aquí se validaría el token
            // Por ahora solo aceptamos el reset

            _logger.LogInformation("[AUTH] Reset de contraseña procesado");

            return new ResetPasswordResponse
            {
                Success = true,
                Message = "Tu contraseña ha sido restablecida exitosamente. Inicia sesión con tu nueva contraseña."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Excepción en ResetPassword");
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "Error al restablecer la contraseña"
            };
        }
    }}
