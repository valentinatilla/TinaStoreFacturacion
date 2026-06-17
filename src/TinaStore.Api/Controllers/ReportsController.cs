using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    /// <summary>Reporte de ventas por rango de fechas.</summary>
    [HttpGet("ventas")]
    public async Task<IActionResult> GetSales([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetSalesReportAsync(from, to));

    /// <summary>Reporte de gastos por rango de fechas.</summary>
    [HttpGet("gastos")]
    public async Task<IActionResult> GetExpenses([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetExpensesReportAsync(from, to));

    /// <summary>Reporte de cuentas por cobrar vigentes.</summary>
    [HttpGet("cuentas-por-cobrar")]
    public async Task<IActionResult> GetReceivables()
        => Ok(await _service.GetReceivablesReportAsync());

    /// <summary>Top productos más vendidos en el período.</summary>
    [HttpGet("top-productos")]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int top = 10)
        => Ok(await _service.GetTopProductsAsync(from, to, top));
}
