using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Exceptions;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenses;
    private readonly IRepository<ExpenseCategory> _categories;

    public ExpenseService(IExpenseRepository expenses, IRepository<ExpenseCategory> categories)
    {
        _expenses = expenses;
        _categories = categories;
    }

    public async Task<IEnumerable<ExpenseDto>> GetAllAsync()
    {
        var lista = await _expenses.GetAllAsync();
        return lista.OrderByDescending(e => e.ExpenseDate).Select(ToDto);
    }

    public async Task<IEnumerable<ExpenseDto>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var lista = await _expenses.GetByDateRangeAsync(from, to.AddDays(1).AddTicks(-1));
        return lista.Select(ToDto);
    }

    public async Task<IEnumerable<ExpenseDto>> GetByCategoryAsync(int categoryId)
    {
        var lista = await _expenses.GetByCategoryAsync(categoryId);
        return lista.Select(ToDto);
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        var e = await _expenses.GetByIdAsync(id);
        return e is null ? null : ToDto(e);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseDto dto)
    {
        var categoria = await _categories.GetByIdAsync(dto.ExpenseCategoryId)
            ?? throw new EntityNotFoundException(nameof(ExpenseCategory), dto.ExpenseCategoryId);

        var entity = new Expense
        {
            ExpenseDate = dto.ExpenseDate,
            Description = dto.Description,
            Amount = dto.Amount,
            Notes = dto.Notes,
            Status = ExpenseStatus.Active,
            ExpenseCategoryId = dto.ExpenseCategoryId,
            ExpenseCategory = categoria,
            SupplierId = dto.SupplierId,
            PaymentMethodId = dto.PaymentMethodId
        };

        await _expenses.AddAsync(entity);
        await _expenses.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<ExpenseDto?> UpdateAsync(int id, UpdateExpenseDto dto)
    {
        var entity = await _expenses.GetByIdAsync(id);
        if (entity is null) return null;
        if (entity.Status == ExpenseStatus.Cancelled)
            throw new DomainException("No se puede modificar un gasto anulado.");

        var categoria = await _categories.GetByIdAsync(dto.ExpenseCategoryId)
            ?? throw new EntityNotFoundException(nameof(ExpenseCategory), dto.ExpenseCategoryId);

        entity.ExpenseDate = dto.ExpenseDate;
        entity.Description = dto.Description;
        entity.Amount = dto.Amount;
        entity.Notes = dto.Notes;
        entity.ExpenseCategoryId = dto.ExpenseCategoryId;
        entity.ExpenseCategory = categoria;
        entity.SupplierId = dto.SupplierId;
        entity.PaymentMethodId = dto.PaymentMethodId;

        await _expenses.UpdateAsync(entity);
        await _expenses.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> CancelAsync(int id)
    {
        var entity = await _expenses.GetByIdAsync(id);
        if (entity is null) return false;
        entity.Status = ExpenseStatus.Cancelled;
        await _expenses.UpdateAsync(entity);
        await _expenses.SaveChangesAsync();
        return true;
    }

    private static ExpenseDto ToDto(Expense e) => new(
        e.Id,
        e.ExpenseDate,
        e.Description,
        e.Amount,
        e.Notes,
        e.Status,
        e.Status.ToString(),
        e.ExpenseCategoryId,
        e.ExpenseCategory?.Name ?? string.Empty,
        e.SupplierId,
        e.Supplier?.Name,
        e.PaymentMethodId,
        e.PaymentMethod?.Name
    );
}
