namespace TinaStore.Application.DTOs;

/// <summary>KPIs principales del dashboard de la tienda.</summary>
public record DashboardDto(
    // ─── Ventas ───────────────────────────────────────────────────────────────
    decimal VentasHoy,
    int FacturasHoy,
    decimal VentasSemana,
    decimal VentasMes,

    // ─── Cobros pendientes ────────────────────────────────────────────────────
    decimal TotalPorCobrar,
    int ClientesConDeuda,

    // ─── Gastos ───────────────────────────────────────────────────────────────
    decimal GastosHoy,
    decimal GastosMes,

    // ─── Inventario ───────────────────────────────────────────────────────────
    int ProductosStockBajo,
    int TotalProductosActivos,

    // ─── Últimas facturas del día ─────────────────────────────────────────────
    List<InvoiceSummaryDto> UltimasFacturas,

    // ─── Deudores con mayor saldo ─────────────────────────────────────────────
    List<DeudorResumenDto> TopDeudores
);

/// <summary>Resumen de deudor para el widget del dashboard.</summary>
public record DeudorResumenDto(
    int CustomerId,
    string CustomerName,
    string? Phone,
    decimal Saldo
);
