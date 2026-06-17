using TinaStore.Domain.Enums;

namespace TinaStore.Domain.Entities;

/// <summary>Registro de cada entrada o salida de inventario. Trazabilidad completa.</summary>
public class InventoryMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public InventoryMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
}
