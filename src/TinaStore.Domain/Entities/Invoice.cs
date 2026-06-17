using TinaStore.Domain.Enums;

namespace TinaStore.Domain.Entities;

/// <summary>Factura de venta. Registra todo lo de una transacción con un cliente.</summary>
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; } = 0;
    public decimal Balance => Total - AmountPaid;

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }

    public ICollection<InvoiceDetail> Details { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = [];
    public AccountReceivable? AccountReceivable { get; set; }
}
