using TinaStore.Domain.Enums;

namespace TinaStore.Domain.Entities;

/// <summary>Usuario del sistema (administrador, vendedor, consulta).</summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Seller;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
