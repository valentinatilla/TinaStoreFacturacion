using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly IRepository<ExpenseCategory> _categories;

    public ExpenseCategoryService(IRepository<ExpenseCategory> categories)
        => _categories = categories;

    public async Task<IEnumerable<ExpenseCategoryDto>> GetAllAsync(bool soloActivas = false)
    {
        var lista = await _categories.GetAllAsync();
        var result = soloActivas ? lista.Where(c => c.IsActive) : lista;
        return result.Select(ToDto);
    }

    public async Task<ExpenseCategoryDto?> GetByIdAsync(int id)
    {
        var e = await _categories.GetByIdAsync(id);
        return e is null ? null : ToDto(e);
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto)
    {
        var entity = new ExpenseCategory { Name = dto.Name, Description = dto.Description, IsActive = true };
        await _categories.AddAsync(entity);
        await _categories.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<ExpenseCategoryDto?> UpdateAsync(int id, UpdateExpenseCategoryDto dto)
    {
        var entity = await _categories.GetByIdAsync(id);
        if (entity is null) return null;
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        await _categories.UpdateAsync(entity);
        await _categories.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _categories.GetByIdAsync(id);
        if (entity is null) return false;
        await _categories.DeleteAsync(entity);
        await _categories.SaveChangesAsync();
        return true;
    }

    private static ExpenseCategoryDto ToDto(ExpenseCategory c) => new(
        c.Id, c.Name, c.Description, c.IsActive,
        c.Expenses?.Count(e => !e.IsDeleted) ?? 0
    );
}
