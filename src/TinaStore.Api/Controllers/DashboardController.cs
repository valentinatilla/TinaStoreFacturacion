using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    /// <summary>Retorna los KPIs principales del negocio (ventas, cobros, gastos, stock).</summary>
    [HttpGet]
    public async Task<IActionResult> GetSummary()
        => Ok(await _service.GetSummaryAsync());
}
