using TinaStore.Domain.Enums;

namespace TinaStore.Application.Helpers;

/// <summary>
/// Centraliza la traducción al español del estado de una factura.
/// Evita duplicar este switch en múltiples servicios.
/// </summary>
public static class InvoiceStatusHelper
{
    public static string EnEspanol(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Paid      => "Pagada",
        InvoiceStatus.Partial   => "Parcial",
        InvoiceStatus.Cancelled => "Anulada",
        _                       => "Pendiente"
    };
}
