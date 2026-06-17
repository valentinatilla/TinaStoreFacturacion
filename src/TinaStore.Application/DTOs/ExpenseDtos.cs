using TinaStore.Domain.Enums;

namespace TinaStore.Application.DTOs;

// ─── Categoría de egreso ─────────────────────────────────────────────────────

public record ExpenseCategoryDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int ExpenseCount
);

public record CreateExpenseCategoryDto(
    string Name,
    string? Description
);

public record UpdateExpenseCategoryDto(
    string Name,
    string? Description,
    bool IsActive
);

// ─── Egreso / Gasto ──────────────────────────────────────────────────────────

public record ExpenseDto(
    int Id,
    DateTime ExpenseDate,
    string Description,
    decimal Amount,
    string? Notes,
    ExpenseStatus Status,
    string StatusName,
    int ExpenseCategoryId,
    string ExpenseCategoryName,
    int? SupplierId,
    string? SupplierName,
    int? PaymentMethodId,
    string? PaymentMethodName
);

public record CreateExpenseDto(
    DateTime ExpenseDate,
    string Description,
    decimal Amount,
    string? Notes,
    int ExpenseCategoryId,
    int? SupplierId,
    int? PaymentMethodId
);

public record UpdateExpenseDto(
    DateTime ExpenseDate,
    string Description,
    decimal Amount,
    string? Notes,
    int ExpenseCategoryId,
    int? SupplierId,
    int? PaymentMethodId
);
