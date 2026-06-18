namespace TinaStore.Application.DTOs;

/// <summary>Configuración completa de la tienda.</summary>
public record StoreSettingsDto(
    int Id,
    string StoreName,
    string? LogoPath,
    string? Address,
    string? Phone,
    string? Email,
    string? TaxId,
    string? InvoiceFooterMessage,
    string? ReminderMessage,
    string Currency,
    decimal TaxPercentage,
    int InvoiceConsecutive,
    bool AllowNegativeStock
);

/// <summary>Datos actualizables de la configuración (no se puede modificar el consecutivo desde aquí).</summary>
public record UpdateStoreSettingsDto(
    string StoreName,
    string? Address,
    string? Phone,
    string? Email,
    string? TaxId,
    string? InvoiceFooterMessage,
    string? ReminderMessage,
    string Currency,
    decimal TaxPercentage,
    bool AllowNegativeStock
);

/// <summary>Configuración completa retornada desde Settings (incluye ReminderMessage).</summary>
public record StoreSettingsFullDto(
    int Id,
    string StoreName,
    string? LogoPath,
    string? Address,
    string? Phone,
    string? Email,
    string? TaxId,
    string? InvoiceFooterMessage,
    string? ReminderMessage,
    string Currency,
    decimal TaxPercentage,
    int InvoiceConsecutive,
    bool AllowNegativeStock
);
