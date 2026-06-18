using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SettingsController : ControllerBase
{
    private readonly IStoreSettingsService _service;

    public SettingsController(IStoreSettingsService service) => _service = service;

    /// <summary>Obtiene la configuración actual de la tienda.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _service.GetAsync());

    /// <summary>Actualiza la configuración de la tienda.</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateStoreSettingsDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.StoreName))
            return BadRequest(new { mensaje = "El nombre de la tienda es obligatorio." });

        var result = await _service.UpdateAsync(dto);
        return Ok(result);
    }
}
