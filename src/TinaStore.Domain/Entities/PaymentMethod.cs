using TinaStore.Domain.Enums;

namespace TinaStore.Domain.Entities;

/// <summary>Método de pago disponible: efectivo, Nequi, transferencia, tarjeta, fiado, etc.</summary>
public class PaymentMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public PaymentMethodType Type { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Expense> Expenses { get; set; } = [];
}
