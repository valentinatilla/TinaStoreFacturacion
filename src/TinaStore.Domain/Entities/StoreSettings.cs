namespace TinaStore.Domain.Entities;

/// <summary>Configuración general de la tienda: nombre, logo, datos de facturación, consecutivo.</summary>
public class StoreSettings : BaseEntity
{
    public string StoreName { get; set; } = "Tina Store";
    public string? LogoPath { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }
    public string? InvoiceFooterMessage { get; set; }
    public string? ReminderMessage { get; set; }
    public string Currency { get; set; } = "COP";
    public decimal TaxPercentage { get; set; } = 0;
    public int InvoiceConsecutive { get; set; } = 1;
    public bool AllowNegativeStock { get; set; } = false;
}
