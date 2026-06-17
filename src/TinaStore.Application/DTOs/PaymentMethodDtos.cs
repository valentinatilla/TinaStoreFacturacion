using TinaStore.Domain.Enums;

namespace TinaStore.Application.DTOs;

/// <summary>DTO de respuesta con los datos de un método de pago.</summary>
public record PaymentMethodDto(
    int Id,
    string Name,
    PaymentMethodType Type,
    string TypeName,
    string? Description,
    bool IsActive
);

/// <summary>DTO para crear un nuevo método de pago.</summary>
public record CreatePaymentMethodDto(
    string Name,
    PaymentMethodType Type,
    string? Description
);

/// <summary>DTO para actualizar un método de pago existente.</summary>
public record UpdatePaymentMethodDto(
    string Name,
    PaymentMethodType Type,
    string? Description,
    bool IsActive
);
