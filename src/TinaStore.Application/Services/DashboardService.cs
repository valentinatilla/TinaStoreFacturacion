using Microsoft.EntityFrameworkCore;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Enums;
using TinaStore.Infrastructure.Data;

namespace TinaStore.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetSummaryAsync()
    {
        var ahora = DateTime.UtcNow;
        var hoyInicio = ahora.Date;
        var hoyFin = hoyInicio.AddDays(1).AddTicks(-1);
        var semanaInicio = hoyInicio.AddDays(-6);
        var mesInicio = new DateTime(ahora.Year, ahora.Month, 1);

        // ── Ventas ────────────────────────────────────────────────────────────
        var facturasHoy = await _db.Invoices
            .Where(i => i.Status != InvoiceStatus.Cancelled
                        && i.InvoiceDate >= hoyInicio
                        && i.InvoiceDate <= hoyFin)
            .ToListAsync();

        var ventasHoy = facturasHoy.Sum(i => i.Total);
        var cantidadHoy = facturasHoy.Count;

        var ventasSemana = await _db.Invoices
            .Where(i => i.Status != InvoiceStatus.Cancelled
                        && i.InvoiceDate >= semanaInicio)
            .SumAsync(i => i.Total);

        var ventasMes = await _db.Invoices
            .Where(i => i.Status != InvoiceStatus.Cancelled
                        && i.InvoiceDate >= mesInicio)
            .SumAsync(i => i.Total);

        // ── Cuentas por cobrar ────────────────────────────────────────────────
        var cxcPendientes = await _db.AccountsReceivable
            .Include(a => a.Customer)
            .Where(a => a.TotalDebt > a.TotalPaid)
            .ToListAsync();

        var totalPorCobrar = cxcPendientes.Sum(a => a.Balance);
        var clientesConDeuda = cxcPendientes.Count;

        // ── Egresos ───────────────────────────────────────────────────────────
        var gastosHoy = await _db.Expenses
            .Where(e => e.Status == ExpenseStatus.Active
                        && e.ExpenseDate >= hoyInicio
                        && e.ExpenseDate <= hoyFin)
            .SumAsync(e => e.Amount);

        var gastosMes = await _db.Expenses
            .Where(e => e.Status == ExpenseStatus.Active
                        && e.ExpenseDate >= mesInicio)
            .SumAsync(e => e.Amount);

        // ── Inventario ────────────────────────────────────────────────────────
        var productosStockBajo = await _db.Products
            .CountAsync(p => p.IsActive && !p.IsDeleted && p.CurrentStock <= p.MinimumStock);

        var totalProductosActivos = await _db.Products
            .CountAsync(p => p.IsActive && !p.IsDeleted);

        // ── Últimas facturas del día ──────────────────────────────────────────
        var ultimasFacturas = facturasHoy
            .OrderByDescending(i => i.InvoiceDate)
            .Take(5)
            .Select(i => new InvoiceSummaryDto(
                i.Id, i.InvoiceNumber, i.InvoiceDate,
                i.Customer?.FullName ?? string.Empty,
                i.Total, i.Balance, i.Status, i.Status.ToString()))
            .ToList();

        // ── Top deudores ──────────────────────────────────────────────────────
        var topDeudores = cxcPendientes
            .OrderByDescending(a => a.Balance)
            .Take(5)
            .Select(a => new DeudorResumenDto(
                a.CustomerId,
                a.Customer?.FullName ?? string.Empty,
                a.Customer?.Phone,
                a.Balance))
            .ToList();

        return new DashboardDto(
            ventasHoy,
            cantidadHoy,
            ventasSemana,
            ventasMes,
            totalPorCobrar,
            clientesConDeuda,
            gastosHoy,
            gastosMes,
            productosStockBajo,
            totalProductosActivos,
            ultimasFacturas,
            topDeudores
        );
    }
}
