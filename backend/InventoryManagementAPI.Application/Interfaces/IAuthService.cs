using InventoryManagementAPI.Application.DTOs.Auth;

namespace InventoryManagementAPI.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Autentica un usuario y genera JWT Token
    /// </summary>
    Task<AuthResponse> LoginAsync(UserLoginRequest request);
    
    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    Task<AuthResponse> RegisterAsync(UserRegisterRequest request);

    /// <summary>
    /// Obtiene información del usuario por ID
    /// </summary>
    Task<UserResponse?> GetUserByIdAsync(int userId);
}
