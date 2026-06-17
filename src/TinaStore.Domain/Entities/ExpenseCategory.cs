namespace TinaStore.Domain.Entities;

/// <summary>Categoría de egreso (gasto operativo, nómina, arriendo, servicios, etc.).</summary>
public class ExpenseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Expense> Expenses { get; set; } = [];
}
