namespace TinaStore.Application.DTOs;

/// <summary>DTO de respuesta con los datos de una categoría.</summary>
public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int ProductCount
);

/// <summary>DTO para crear una nueva categoría.</summary>
public record CreateCategoryDto(
    string Name,
    string? Description
);

/// <summary>DTO para actualizar una categoría existente.</summary>
public record UpdateCategoryDto(
    string Name,
    string? Description,
    bool IsActive
);
