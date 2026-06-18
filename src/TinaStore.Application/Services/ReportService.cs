using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo) => _repo = repo;

    public async Task<ReporteVentasDto> GetSalesReportAsync(DateTime from, DateTime to)
    {
        var toFin = to.Date.AddDays(1).AddTicks(-1);
        var facturas = await _repo.GetInvoicesByRangeAsync(from.Date, toFin);
        var detalles = await _repo.GetSoldDetailsByRangeAsync(from.Date, toFin);

        var totalVentas = facturas.Sum(i => i.Total);
        var totalCobrado = facturas.Sum(i => i.AmountPaid);
        var totalPendiente = facturas.Sum(i => i.Balance);

        var ventasPorDia = facturas
            .GroupBy(i => i.InvoiceDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new VentasPorPeriodoDto(
                g.Key,
                g.Count(),
                g.Sum(i => i.Total),
                g.Sum(i => i.AmountPaid),
                g.Sum(i => i.Balance)))
            .ToList();

        var topProductos = detalles
            .GroupBy(d => new { d.ProductId, d.ProductName, Sku = d.Product?.InternalCode })
            .OrderByDescending(g => g.Sum(d => d.Quantity))
            .Take(10)
            .Select(g => new TopProductoDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.Sku,
                g.Sum(d => d.Quantity),
                g.Sum(d => d.Subtotal)))
            .ToList();

        return new ReporteVentasDto(from, to, totalVentas, totalCobrado, totalPendiente,
            facturas.Count, ventasPorDia, topProductos);
    }

    public async Task<ReporteGastosDto> GetExpensesReportAsync(DateTime from, DateTime to)
    {
        var toFin = to.Date.AddDays(1).AddTicks(-1);
        var gastos = await _repo.GetExpensesByRangeAsync(from.Date, toFin);

        var totalMonto = gastos.Sum(e => e.Amount);

        var porCategoria = gastos
            .GroupBy(e => new { e.ExpenseCategoryId, Name = e.ExpenseCategory?.Name ?? "Sin categoría" })
            .OrderByDescending(g => g.Sum(e => e.Amount))
            .Select(g => new ResumenGastosPorCategoriaDto(
                g.Key.ExpenseCategoryId,
                g.Key.Name,
                g.Count(),
                g.Sum(e => e.Amount)))
            .ToList();

        return new ReporteGastosDto(from, to, totalMonto, gastos.Count, porCategoria);
    }

    public async Task<ReporteCuentasPorCobrarDto> GetReceivablesReportAsync()
    {
        var cuentas = await _repo.GetAllReceivablesAsync();
        var total   = cuentas.Sum(a => a.Balance);

        var deudores = cuentas
            .Select(a =>
            {
                var facturasPendientes = a.Customer?.Invoices?
                    .Count(i => i.Balance > 0 && i.Status != InvoiceStatus.Cancelled) ?? 0;
                var ultimaFecha = a.Customer?.Invoices?
                    .Where(i => i.Balance > 0 && i.Status != InvoiceStatus.Cancelled)
                    .OrderByDescending(i => i.InvoiceDate)
                    .FirstOrDefault()?.InvoiceDate;

                return new DeudorCXCDto(
                    a.CustomerId,
                    a.Customer?.FullName    ?? string.Empty,
                    a.Customer?.DocumentNumber,
                    a.Customer?.Phone,
                    a.Balance,
                    facturasPendientes,
                    ultimaFecha);
            })
            .ToList();

        return new ReporteCuentasPorCobrarDto(total, cuentas.Count, deudores);
    }

    public async Task<IEnumerable<TopProductoDto>> GetTopProductsAsync(DateTime from, DateTime to, int top = 10)
    {
        var toFin = to.Date.AddDays(1).AddTicks(-1);
        var detalles = await _repo.GetSoldDetailsByRangeAsync(from.Date, toFin);

        return detalles
            .GroupBy(d => new { d.ProductId, d.ProductName, Sku = d.Product?.InternalCode })
            .OrderByDescending(g => g.Sum(d => d.Quantity))
            .Take(top)
            .Select(g => new TopProductoDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.Sku,
                g.Sum(d => d.Quantity),
                g.Sum(d => d.Subtotal)));
    }
}
