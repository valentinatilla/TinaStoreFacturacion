namespace TinaStore.Domain.Entities;

/// <summary>Línea de detalle de una factura: producto, cantidad, precio y subtotal.</summary>
public class InvoiceDetail : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>Nombre del producto o descripción libre para líneas sin producto de inventario.</summary>
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal Subtotal => (UnitPrice * Quantity) - DiscountAmount;
}
