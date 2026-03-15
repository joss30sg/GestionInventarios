using Microsoft.AspNetCore.Authorization;
using InventoryManagementAPI.Domain.Entities;

namespace InventoryManagementAPI.Api.Authorization;

/// <summary>
/// Atributo personalizado para autorización basada en roles
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
    {
        var roleNames = allowedRoles.Select(r => r.ToString()).ToList();
        Roles = string.Join(",", roleNames);
    }
}
