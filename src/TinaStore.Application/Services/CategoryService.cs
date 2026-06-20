using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;

    public CategoryService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(bool soloActivas = false)
    {
        var lista = await _categories.GetAllWithProductsAsync();
        var resultado = soloActivas ? lista.Where(c => c.IsActive) : lista;
        return resultado.Select(ToDto);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var entity = await _categories.GetByIdAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var entity = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true
        };

        await _categories.AddAsync(entity);
        await _categories.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
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

    private static CategoryDto ToDto(Category c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.IsActive,
        c.Products?.Count ?? 0
    );
}
