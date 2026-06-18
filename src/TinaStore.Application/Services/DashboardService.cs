using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;
    private readonly IAppClock _clock;

    public DashboardService(IDashboardRepository repo, IAppClock clock)
    {
        _repo = repo;
        _clock = clock;
    }

    public async Task<DashboardDto> GetSummaryAsync()
    {
        var ahora = _clock.Now;
        var hoyInicio = ahora.Date;
        var hoyFin = hoyInicio.AddDays(1).AddTicks(-1);
        var semanaInicio = hoyInicio.AddDays(-6);
        var mesInicio = new DateTime(ahora.Year, ahora.Month, 1);

        var ventasHoy = await _repo.GetSalesTodayAsync(hoyInicio, hoyFin);
        var cantidadHoy = await _repo.GetInvoiceCountTodayAsync(hoyInicio, hoyFin);
        var ventasSemana = await _repo.GetSalesWeekAsync(semanaInicio);
        var ventasMes = await _repo.GetSalesMonthAsync(mesInicio);
        var totalPorCobrar = await _repo.GetTotalReceivableAsync();
        var clientesConDeuda = await _repo.GetCustomersWithDebtAsync();
        var gastosHoy = await _repo.GetExpensesTodayAsync(hoyInicio, hoyFin);
        var gastosMes = await _repo.GetExpensesMonthAsync(mesInicio);
        var productosStockBajo = await _repo.GetLowStockCountAsync();
        var totalProductosActivos = await _repo.GetActiveProductCountAsync();

        var ultimasFacturas = (await _repo.GetLastInvoicesTodayAsync(hoyInicio, hoyFin, 5))
            .Select(i => new InvoiceSummaryDto(
                i.Id, i.InvoiceNumber, i.InvoiceDate,
                i.Customer?.FullName ?? string.Empty,
                i.Total, i.Balance, i.Status, i.Status.ToString()))
            .ToList();

        var topDeudores = (await _repo.GetTopDebtorsAsync(5))
            .Select(a => new DeudorResumenDto(
                a.CustomerId,
                a.Customer?.FullName ?? string.Empty,
                a.Customer?.Phone,
                a.Balance))
            .ToList();

        return new DashboardDto(
            ventasHoy, cantidadHoy, ventasSemana, ventasMes,
            totalPorCobrar, clientesConDeuda,
            gastosHoy, gastosMes,
            productosStockBajo, totalProductosActivos,
            ultimasFacturas, topDeudores);
    }
}

