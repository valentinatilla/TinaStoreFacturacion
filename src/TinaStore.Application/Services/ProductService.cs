using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Exceptions;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class ProductService : IProductService
{
    private const string CategoriaNombreCompras = "Compras a proveedor";

    private readonly IProductRepository _products;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Supplier> _suppliers;
    private readonly IExpenseRepository _expenses;
    private readonly IRepository<ExpenseCategory> _expenseCategories;
    private readonly IRepository<InventoryMovement> _movements;
    private readonly IAppClock _clock;

    public ProductService(
        IProductRepository products,
        IRepository<Category> categories,
        IRepository<Supplier> suppliers,
        IExpenseRepository expenses,
        IRepository<ExpenseCategory> expenseCategories,
        IRepository<InventoryMovement> movements,
        IAppClock clock)
    {
        _products = products;
        _categories = categories;
        _suppliers = suppliers;
        _expenses = expenses;
        _expenseCategories = expenseCategories;
        _movements = movements;
        _clock = clock;
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
        // Validar unicidad de nombre
        var existeNombre = await _products.FindByNameAsync(dto.Name);
        if (existeNombre is not null)
            throw new DomainException($"Ya existe un producto con el nombre '{dto.Name.Trim()}'.");

        // Validar unicidad de SKU
        if (!string.IsNullOrWhiteSpace(dto.Sku))
        {
            var skuTrim = dto.Sku.Trim();
            var existeSku = await _products.FindAsync(
                p => p.Sku != null && p.Sku.ToLower() == skuTrim.ToLower());
            if (existeSku.Any())
                throw new DomainException($"Ya existe un producto con el SKU '{skuTrim}'.");
        }

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

        if (dto.PurchasePrice > 0 && dto.CurrentStock > 0)
            await RegistrarEgresoCompraAsync(entity, dto.CurrentStock, dto.PurchasePrice);

        return ToDto(entity);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var entity = await _products.GetByIdAsync(id);
        if (entity is null) return null;

        // Validar unicidad de nombre (excluir el propio producto)
        var existeNombre = await _products.FindByNameAsync(dto.Name, excludeId: id);
        if (existeNombre is not null)
            throw new DomainException($"Ya existe un producto con el nombre '{dto.Name.Trim()}'.");

        // Validar unicidad de SKU (excluir el propio producto)
        if (!string.IsNullOrWhiteSpace(dto.Sku))
        {
            var skuTrim = dto.Sku.Trim();
            var existeSku = await _products.FindAsync(
                p => p.Id != id && p.Sku != null && p.Sku.ToLower() == skuTrim.ToLower());
            if (existeSku.Any())
                throw new DomainException($"Ya existe un producto con el SKU '{skuTrim}'.");
        }

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

        if (dto.StockEntrada != 0)
            entity.CurrentStock += dto.StockEntrada;

        await _products.UpdateAsync(entity);
        await _products.SaveChangesAsync();

        if (dto.StockEntrada > 0 && dto.PurchasePrice > 0)
            await RegistrarEgresoCompraAsync(entity, dto.StockEntrada, dto.PurchasePrice);

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

    public async Task<ProductDto?> AjustarStockAsync(int id, AjusteStockDto dto)
    {
        if (dto.Cantidad <= 0)
            throw new DomainException("La cantidad del ajuste debe ser mayor a cero.");

        var entity = await _products.GetByIdAsync(id);
        if (entity is null) return null;

        var stockAntes = entity.CurrentStock;
        entity.CurrentStock += dto.Cantidad;

        var movimiento = new InventoryMovement
        {
            ProductId    = entity.Id,
            MovementType = InventoryMovementType.Entry,
            Quantity     = dto.Cantidad,
            StockBefore  = stockAntes,
            StockAfter   = entity.CurrentStock,
            Notes        = dto.Notas ?? "Entrada rápida desde factura"
        };

        await _products.UpdateAsync(entity);
        await _movements.AddAsync(movimiento);
        await _products.SaveChangesAsync();

        // Registrar egreso de compra si el producto tiene precio de costo
        if (entity.PurchasePrice > 0)
            await RegistrarEgresoCompraAsync(entity, dto.Cantidad, entity.PurchasePrice);

        return ToDto(entity);
    }

    public async Task<BulkUpdateResultDto> BulkUpdateAsync(IEnumerable<BulkUpdateItemDto> items)
    {
        var resultados = new List<BulkUpdateItemResultDto>();
        var lista      = items.ToList();

        foreach (var item in lista)
        {
            var entity = await _products.GetByIdAsync(item.ProductId);
            if (entity is null)
            {
                resultados.Add(new BulkUpdateItemResultDto(item.ProductId, $"ID {item.ProductId}", false, "Producto no encontrado."));
                continue;
            }

            // ── Validaciones ──────────────────────────────────────────────────
            if (item.NuevoCosto.HasValue && item.NuevoCosto.Value < 0)
            {
                resultados.Add(new BulkUpdateItemResultDto(entity.Id, entity.Name, false, "El costo no puede ser negativo."));
                continue;
            }
            if (item.NuevoPrecioVenta.HasValue && item.NuevoPrecioVenta.Value < 0)
            {
                resultados.Add(new BulkUpdateItemResultDto(entity.Id, entity.Name, false, "El precio de venta no puede ser negativo."));
                continue;
            }
            if (item.NuevoStock.HasValue && item.NuevoStock.Value < 0)
            {
                resultados.Add(new BulkUpdateItemResultDto(entity.Id, entity.Name, false, "El stock no puede ser negativo."));
                continue;
            }

            // ── Aplicar cambios ───────────────────────────────────────────────
            if (item.NuevoCosto.HasValue)
                entity.PurchasePrice = item.NuevoCosto.Value;

            if (item.NuevoPrecioVenta.HasValue)
                entity.SalePrice = item.NuevoPrecioVenta.Value;

            if (item.NuevoStock.HasValue && item.NuevoStock.Value != entity.CurrentStock)
            {
                var stockAntes = entity.CurrentStock;
                var diferencia = item.NuevoStock.Value - stockAntes;

                entity.CurrentStock = item.NuevoStock.Value;

                await _movements.AddAsync(new InventoryMovement
                {
                    ProductId    = entity.Id,
                    MovementType = InventoryMovementType.Adjustment,
                    Quantity     = Math.Abs(diferencia),
                    StockBefore  = stockAntes,
                    StockAfter   = entity.CurrentStock,
                    Notes        = "Ajuste por edición masiva"
                });
            }

            if (item.NuevaCategoriaId.HasValue && item.NuevaCategoriaId.Value != entity.CategoryId)
            {
                var cat = await _categories.GetByIdAsync(item.NuevaCategoriaId.Value);
                if (cat is not null)
                {
                    entity.CategoryId = cat.Id;
                    entity.Category   = cat;
                }
            }

            if (item.LimpiarProveedor)
            {
                entity.SupplierId = null;
            }
            else if (item.NuevoProveedorId.HasValue && item.NuevoProveedorId.Value != entity.SupplierId)
            {
                var prov = await _suppliers.GetByIdAsync(item.NuevoProveedorId.Value);
                if (prov is not null)
                    entity.SupplierId = prov.Id;
            }

            await _products.UpdateAsync(entity);
            resultados.Add(new BulkUpdateItemResultDto(entity.Id, entity.Name, true, null));
        }

        // Un único SaveChanges para todo el lote
        await _products.SaveChangesAsync();

        return new BulkUpdateResultDto(
            lista.Count,
            resultados.Count(r => r.Ok),
            resultados.Count(r => !r.Ok),
            resultados
        );
    }

    private async Task RegistrarEgresoCompraAsync(Product product, int cantidad, decimal precioUnitario)
    {
        // Busca la categoría por nombre directamente
        var resultados = await _expenseCategories.FindAsync(c =>
            c.Name == CategoriaNombreCompras && !c.IsDeleted);
        var categoria = resultados.FirstOrDefault();
        if (categoria is null) return;

        var egreso = new Expense
        {
            ExpenseDate       = _clock.Now,
            Description       = $"Compra de {cantidad} unidad(es) de '{product.Name}'",
            Amount            = precioUnitario * cantidad,
            Notes             = product.Sku is not null ? $"SKU: {product.Sku}" : null,
            Status            = ExpenseStatus.Active,
            ExpenseCategoryId = categoria.Id,
            SupplierId        = product.SupplierId,
            ProductId         = product.Id,
            StockQty          = cantidad
        };

        await _expenses.AddAsync(egreso);
        await _expenses.SaveChangesAsync();
    }

    private static ProductSummaryDto ToSummaryDto(Product p) => new(
        p.Id,
        p.Sku,
        p.Name,
        p.SalePrice,
        p.PurchasePrice,
        p.ProfitMargin,
        p.CurrentStock,
        p.IsLowStock,
        p.IsActive,
        p.CategoryId,
        p.Category?.Name ?? string.Empty,
        p.SupplierId,
        p.Supplier?.Name,
        p.ImagePath,
        p.CreatedAt
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
