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
        SalePrice > 0 && PurchasePrice > 0
            ? Math.Round((SalePrice - PurchasePrice) / SalePrice * 100, 2)
            : 0;

    public bool IsLowStock => CurrentStock <= MinimumStock;
}
