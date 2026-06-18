namespace TinaStore.Application.DTOs;

/// <summary>DTO de respuesta con los datos de un cliente.</summary>
public record CustomerDto(
    int Id,
    string FullName,
    string? DocumentType,
    string? DocumentNumber,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes,
    bool IsActive,
    decimal PendingBalance,
    DateTime CreatedAt,
    DateTime? LastPurchaseDate,
    string CommercialStatus
);

/// <summary>DTO para crear un nuevo cliente.</summary>
public record CreateCustomerDto(
    string FullName,
    string? DocumentType,
    string? DocumentNumber,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes
);

/// <summary>DTO para actualizar datos de un cliente existente.</summary>
public record UpdateCustomerDto(
    string FullName,
    string? DocumentType,
    string? DocumentNumber,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes,
    bool IsActive
);
