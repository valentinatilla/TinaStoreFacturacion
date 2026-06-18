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

    /// <summary>Sube el logo de la tienda (multipart/form-data, campo: file).</summary>
    [HttpPost("logo")]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { mensaje = "Debes enviar un archivo de imagen." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
            return BadRequest(new { mensaje = "Solo se admiten imágenes JPG, PNG o WebP." });

        await using var stream = file.OpenReadStream();
        var result = await _service.UploadLogoAsync(stream, file.FileName);
        return Ok(result);
    }
}
