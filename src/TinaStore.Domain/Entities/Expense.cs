using TinaStore.Domain.Enums;

namespace TinaStore.Domain.Entities;

/// <summary>Registro de un gasto o egreso de la tienda (arriendo, servicios, compras, etc.).</summary>
public class Expense : BaseEntity
{
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Active;

    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory ExpenseCategory { get; set; } = null!;

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>Producto asociado a esta compra (para revertir stock al anular).</summary>
    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>Cantidad de stock comprada (para revertir al anular).</summary>
    public int? StockQty { get; set; }
}
