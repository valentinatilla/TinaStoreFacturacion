namespace TinaStore.Application.DTOs;

public record VentasPorPeriodoDto(
    DateTime Fecha,
    int CantidadFacturas,
    decimal TotalVentas,
    decimal TotalCobrado,
    decimal TotalPendiente
);

public record TopProductoDto(
    int ProductId,
    string ProductName,
    string? Sku,
    int TotalVendido,
    decimal TotalIngresos
);

public record ResumenGastosPorCategoriaDto(
    int CategoryId,
    string CategoryName,
    int TotalEgresos,
    decimal TotalMonto
);

public record ReporteVentasDto(
    DateTime Desde,
    DateTime Hasta,
    decimal TotalVentas,
    decimal TotalCobrado,
    decimal TotalPendiente,
    int TotalFacturas,
    List<VentasPorPeriodoDto> VentasPorDia,
    List<TopProductoDto> TopProductos
);

public record ReporteGastosDto(
    DateTime Desde,
    DateTime Hasta,
    decimal TotalGastos,
    int TotalEgresos,
    List<ResumenGastosPorCategoriaDto> PorCategoria
);

public record ReporteCuentasPorCobrarDto(
    decimal TotalPorCobrar,
    int TotalClientes,
    List<DeudorCXCDto> Deudores
);

/// <summary>Detalle de un deudor en el reporte de cuentas por cobrar.</summary>
public record DeudorCXCDto(
    int CustomerId,
    string CustomerName,
    string? DocumentNumber,
    string? Phone,
    decimal SaldoPendiente,
    int FacturasPendientes,
    DateTime? UltimaFacturaFecha
);

public record ExcelImportResultDto(
    int TotalFilas,
    int Importados,
    int Errores,
    List<string> MensajesError
);
