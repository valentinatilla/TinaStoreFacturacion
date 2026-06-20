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
    List<DeudorResumenDto> TopDeudores,

    // ─── Producto estrella del mes ────────────────────────────────────────────
    ProductoEstrellaDto? ProductoEstrella,

    // ─── Tendencia de ventas: últimos 7 días (día 0 = hace 6 días, día 6 = hoy)
    List<VentaDiariaDto> VentasUltimos7Dias
);

/// <summary>Resumen de deudor para el widget del dashboard.</summary>
public record DeudorResumenDto(
    int CustomerId,
    string CustomerName,
    string? Phone,
    decimal Saldo
);

/// <summary>Producto más vendido en el mes actual.</summary>
public record ProductoEstrellaDto(
    int ProductId,
    string ProductName,
    string? Sku,
    int UnidadesVendidas,
    decimal TotalIngresos
);

/// <summary>Venta total de un día específico para el mini gráfico de tendencia.</summary>
public record VentaDiariaDto(
    DateTime Fecha,
    decimal Total
);
