namespace TinaStore.Application.DTOs;

/// <summary>DTO de respuesta completo de un producto.</summary>
public record ProductDto(
    int Id,
    string? Sku,
    string Name,
    string? Description,
    string? Unit,
    decimal PurchasePrice,
    decimal SalePrice,
    int CurrentStock,
    int MinimumStock,
    bool IsActive,
    bool IsLowStock,
    decimal ProfitMargin,
    int CategoryId,
    string CategoryName,
    int? SupplierId,
    string? SupplierName,
    DateTime CreatedAt,
    string? ImagePath
);

/// <summary>DTO resumido para listados de productos.</summary>
public record ProductSummaryDto(
    int Id,
    string? Sku,
    string Name,
    decimal SalePrice,
    decimal PurchasePrice,
    decimal ProfitMargin,
    int CurrentStock,
    bool IsLowStock,
    bool IsActive,
    string CategoryName,
    string? ImagePath
);

/// <summary>DTO para crear un nuevo producto.</summary>
public record CreateProductDto(
    string? Sku,
    string Name,
    string? Description,
    string? Unit,
    decimal PurchasePrice,
    decimal SalePrice,
    int CurrentStock,
    int MinimumStock,
    int CategoryId,
    int? SupplierId
);

/// <summary>DTO para actualizar un producto existente.</summary>
public record UpdateProductDto(
    string? Sku,
    string Name,
    string? Description,
    string? Unit,
    decimal PurchasePrice,
    decimal SalePrice,
    int MinimumStock,
    bool IsActive,
    int CategoryId,
    int? SupplierId,
    /// <summary>Unidades adicionales compradas en esta edición. Si > 0 se suma al stock y genera egreso automático.</summary>
    int StockEntrada = 0
);
