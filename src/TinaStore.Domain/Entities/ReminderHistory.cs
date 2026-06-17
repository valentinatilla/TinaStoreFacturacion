using TinaStore.Domain.Enums;

namespace TinaStore.Domain.Entities;

/// <summary>Historial de cada recordatorio enviado: cuándo, a quién y por qué canal.</summary>
public class ReminderHistory : BaseEntity
{
    public int ReminderId { get; set; }
    public Reminder Reminder { get; set; } = null!;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public ReminderChannel Channel { get; set; }
    public ReminderStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
}
