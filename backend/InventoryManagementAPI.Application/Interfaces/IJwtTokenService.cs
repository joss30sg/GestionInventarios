namespace InventoryManagementAPI.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string username, string role = "User");
    bool ValidateToken(string token);
}
