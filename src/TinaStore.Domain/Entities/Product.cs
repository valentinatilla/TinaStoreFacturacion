namespace TinaStore.Domain.Entities;

/// <summary>Producto del inventario con precios, stock y relación con categoría y proveedor.</summary>
public class Product : BaseEntity
{
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int CurrentStock { get; set; } = 0;
    public int MinimumStock { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? ImagePath { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = [];
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = [];

    public decimal ProfitMargin =>
        PurchasePrice > 0
            ? Math.Round((SalePrice - PurchasePrice) / PurchasePrice * 100, 2)
            : 0;

    /// <summary>
    /// Verdadero si hay stock pero está por debajo del mínimo.
    /// Agotado (stock = 0) NO cuenta como bajo stock.
    /// </summary>
    public bool IsLowStock => CurrentStock > 0 && CurrentStock <= MinimumStock;
}
