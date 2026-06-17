using TinaStore.Domain.Enums;

namespace TinaStore.Domain.Entities;

/// <summary>Configuración de recordatorio automático para un cliente con saldo pendiente.</summary>
public class Reminder : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
    public ReminderChannel Channel { get; set; } = ReminderChannel.Email;
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int FrequencyDays { get; set; } = 7;

    public ICollection<ReminderHistory> History { get; set; } = [];
}
