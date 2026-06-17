namespace TinaStore.Domain.Entities;

/// <summary>Resumen de la deuda total de un cliente. Se actualiza con cada factura y abono.</summary>
public class AccountReceivable : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public decimal TotalDebt { get; set; } = 0;
    public decimal TotalPaid { get; set; } = 0;
    public decimal Balance => TotalDebt - TotalPaid;

    public DateTime LastPaymentDate { get; set; }
    public bool IsPaid => Balance <= 0;
}
