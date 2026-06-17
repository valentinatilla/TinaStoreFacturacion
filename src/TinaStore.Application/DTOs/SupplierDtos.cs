namespace TinaStore.Application.DTOs;

/// <summary>DTO de respuesta con los datos de un proveedor.</summary>
public record SupplierDto(
    int Id,
    string Name,
    string? TaxId,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes,
    bool IsActive,
    int ProductCount,
    DateTime CreatedAt
);

/// <summary>DTO para crear un nuevo proveedor.</summary>
public record CreateSupplierDto(
    string Name,
    string? TaxId,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes
);

/// <summary>DTO para actualizar un proveedor existente.</summary>
public record UpdateSupplierDto(
    string Name,
    string? TaxId,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes,
    bool IsActive
);
