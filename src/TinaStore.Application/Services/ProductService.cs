using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Exceptions;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Supplier> _suppliers;

    public ProductService(
        IProductRepository products,
        IRepository<Category> categories,
        IRepository<Supplier> suppliers)
    {
        _products = products;
        _categories = categories;
        _suppliers = suppliers;
    }

    public async Task<IEnumerable<ProductSummaryDto>> GetAllAsync(bool soloActivos = false)
    {
        var lista = await _products.GetAllWithNavigationAsync();
        if (soloActivos) lista = lista.Where(p => p.IsActive).ToList();
        return lista.Select(ToSummaryDto);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var entity = await _products.GetByIdAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<IEnumerable<ProductSummaryDto>> GetLowStockAsync()
    {
        var lista = await _products.GetLowStockAsync();
        return lista.Select(ToSummaryDto);
    }

    public async Task<IEnumerable<ProductSummaryDto>> SearchAsync(string termino)
    {
        var lista = await _products.SearchAsync(termino);
        return lista.Select(ToSummaryDto);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var categoria = await _categories.GetByIdAsync(dto.CategoryId)
            ?? throw new EntityNotFoundException(nameof(Category), dto.CategoryId);

        Supplier? proveedor = null;
        if (dto.SupplierId.HasValue)
        {
            proveedor = await _suppliers.GetByIdAsync(dto.SupplierId.Value)
                ?? throw new EntityNotFoundException(nameof(Supplier), dto.SupplierId.Value);
        }

        var entity = new Product
        {
            Sku = dto.Sku,
            Name = dto.Name,
            Description = dto.Description,
            Unit = dto.Unit,
            PurchasePrice = dto.PurchasePrice,
            SalePrice = dto.SalePrice,
            CurrentStock = dto.CurrentStock,
            MinimumStock = dto.MinimumStock,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId,
            IsActive = true,
            Category = categoria,
            Supplier = proveedor
        };

        await _products.AddAsync(entity);
        await _products.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var entity = await _products.GetByIdAsync(id);
        if (entity is null) return null;

        var categoria = await _categories.GetByIdAsync(dto.CategoryId)
            ?? throw new EntityNotFoundException(nameof(Category), dto.CategoryId);

        entity.Sku = dto.Sku;
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Unit = dto.Unit;
        entity.PurchasePrice = dto.PurchasePrice;
        entity.SalePrice = dto.SalePrice;
        entity.MinimumStock = dto.MinimumStock;
        entity.IsActive = dto.IsActive;
        entity.CategoryId = dto.CategoryId;
        entity.Category = categoria;
        entity.SupplierId = dto.SupplierId;

        await _products.UpdateAsync(entity);
        await _products.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _products.GetByIdAsync(id);
        if (entity is null) return false;

        await _products.DeleteAsync(entity);
        await _products.SaveChangesAsync();
        return true;
    }

    public async Task<ProductDto?> UpdateImagePathAsync(int id, string? imagePath)
    {
        var entity = await _products.GetByIdAsync(id);
        if (entity is null) return null;

        entity.ImagePath = imagePath;
        await _products.UpdateAsync(entity);
        await _products.SaveChangesAsync();
        return ToDto(entity);
    }

    private static ProductSummaryDto ToSummaryDto(Product p) => new(
        p.Id,
        p.Sku,
        p.Name,
        p.SalePrice,
        p.CurrentStock,
        p.IsLowStock,
        p.IsActive,
        p.Category?.Name ?? string.Empty,
        p.ImagePath
    );

    private static ProductDto ToDto(Product p) => new(
        p.Id,
        p.Sku,
        p.Name,
        p.Description,
        p.Unit,
        p.PurchasePrice,
        p.SalePrice,
        p.CurrentStock,
        p.MinimumStock,
        p.IsActive,
        p.IsLowStock,
        p.ProfitMargin,
        p.CategoryId,
        p.Category?.Name ?? string.Empty,
        p.SupplierId,
        p.Supplier?.Name,
        p.CreatedAt,
        p.ImagePath
    );
}
