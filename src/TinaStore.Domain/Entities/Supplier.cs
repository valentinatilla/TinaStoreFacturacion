namespace TinaStore.Domain.Entities;

/// <summary>Proveedor de productos. Puede tener compras y saldo pendiente.</summary>
public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Expense> Expenses { get; set; } = [];
}
