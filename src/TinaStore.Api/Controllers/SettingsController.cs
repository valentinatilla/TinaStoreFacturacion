using FluentValidation;
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
    private static readonly string[] _extensionesLogo = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly (byte[] Firma, string Desc)[] _firmasImagen =
    [
        (new byte[] { 0xFF, 0xD8, 0xFF },       "JPEG"),
        (new byte[] { 0x89, 0x50, 0x4E, 0x47 }, "PNG"),
        (new byte[] { 0x52, 0x49, 0x46, 0x46 }, "WEBP"),
    ];

    private readonly IStoreSettingsService _service;
    private readonly IValidator<UpdateStoreSettingsDto> _updateValidator;

    public SettingsController(IStoreSettingsService service, IValidator<UpdateStoreSettingsDto> updateValidator)
    {
        _service = service;
        _updateValidator = updateValidator;
    }

    /// <summary>Obtiene la configuración actual de la tienda.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _service.GetAsync());

    /// <summary>Actualiza la configuración de la tienda.</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateStoreSettingsDto dto)
    {
        var validacion = await _updateValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

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
        if (!_extensionesLogo.Contains(ext))
            return BadRequest(new { mensaje = "Solo se admiten imágenes JPG, PNG o WebP." });

        // Validar contenido real del archivo (magic bytes)
        var buffer = new byte[8];
        await using var stream = file.OpenReadStream();
        var leidos = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
        var esValido = leidos >= 4 && _firmasImagen.Any(f => buffer.Take(f.Firma.Length).SequenceEqual(f.Firma));
        if (!esValido)
            return BadRequest(new { mensaje = "El archivo no es una imagen válida (JPG, PNG o WebP)." });

        stream.Position = 0;
        var result = await _service.UploadLogoAsync(stream, file.FileName);
        return Ok(result);
    }
}
