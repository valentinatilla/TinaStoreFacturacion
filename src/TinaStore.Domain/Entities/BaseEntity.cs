namespace TinaStore.Domain.Entities;

/// <summary>
/// Clase base de la que heredan todas las entidades del sistema.
/// Proporciona Id, fechas de auditoría y control de borrado lógico.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
