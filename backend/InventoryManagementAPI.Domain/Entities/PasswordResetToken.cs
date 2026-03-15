using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementAPI.Domain.Entities;

/// <summary>
/// Token temporal para recuperación de contraseña olvidada.
/// Cada token tiene una expiración limitada (1 hora típicamente).
/// </summary>
[Table("PasswordResetTokens", Schema = "dbo")]
public class PasswordResetToken
{
    /// <summary>Identificador único del token</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>ID del usuario que solicita reset</summary>
    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    /// <summary>Usuario asociado a este token</summary>
    public required User User { get; set; }

    /// <summary>Token codificado en Base64 (JWT o GUID)</summary>
    [Required]
    [StringLength(500)]
    public required string Token { get; set; }

    /// <summary>Fecha y hora de creación del token (UTC)</summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Fecha y hora de expiración del token (UTC)</summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    /// <summary>Si el token ya fue usado (para evitar reutilización)</summary>
    [Required]
    public bool IsUsed { get; set; } = false;

    /// <summary>Fecha y hora cuando se usó el token (si aplica)</summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>Dirección IP desde la que se creó el token (auditoría)</summary>
    [StringLength(50)]
    public string? CreatedFromIp { get; set; }

    /// <summary>Dirección IP desde la que se usó el token (auditoría)</summary>
    [StringLength(50)]
    public string? UsedFromIp { get; set; }

    /// <summary>Indica si el token es válido (no expirado y no usado)</summary>
    [NotMapped]
    public bool IsValid => !IsUsed && DateTime.UtcNow <= ExpiresAt;
}
