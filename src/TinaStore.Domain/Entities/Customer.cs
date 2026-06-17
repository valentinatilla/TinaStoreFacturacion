namespace TinaStore.Domain.Entities;

/// <summary>Cliente de la tienda. Puede tener saldo pendiente (fiado).</summary>
public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Invoice> Invoices { get; set; } = [];
    public AccountReceivable? AccountReceivable { get; set; }
    public ICollection<Reminder> Reminders { get; set; } = [];
}
