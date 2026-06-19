using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RemindersController : ControllerBase
{
    private readonly IReminderService _reminders;

    public RemindersController(IReminderService reminders) => _reminders = reminders;

    /// <summary>Registra en BD que se generó un recordatorio por WhatsApp para un cliente.</summary>
    [HttpPost("whatsapp")]
    public async Task<IActionResult> RegistrarWhatsApp([FromBody] RegistrarRecordatorioWhatsAppDto dto)
    {
        if (dto.CustomerId <= 0 || string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(new { mensaje = "CustomerId y Message son requeridos." });
        try
        {
            var result = await _reminders.RegistrarRecordatorioWhatsAppAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>Obtiene el historial de recordatorios enviados a un cliente.</summary>
    [HttpGet("historial/{customerId:int}")]
    public async Task<IActionResult> ObtenerHistorial(int customerId)
    {
        var historial = await _reminders.GetHistorialAsync(customerId);
        return Ok(historial);
    }
}
