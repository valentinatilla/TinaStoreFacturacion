using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    private readonly UploadsSettings _uploads;

    public SettingsController(IStoreSettingsService service, IValidator<UpdateStoreSettingsDto> updateValidator, IOptions<UploadsSettings> uploads)
    {
        _service = service;
        _updateValidator = updateValidator;
        _uploads = uploads.Value;
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

    /// <summary>Devuelve el archivo de logo actual. No requiere autenticación para que el browser pueda cargarlo directamente.</summary>
    [HttpGet("logo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLogo()
    {
        var cfg = await _service.GetAsync();
        if (string.IsNullOrEmpty(cfg.LogoPath))
            return NotFound();

        var rutaRelativa = cfg.LogoPath.TrimStart('/');
        var rutaFisica   = Path.Combine(_uploads.BasePath.Length > 0 ? _uploads.BasePath : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), rutaRelativa);
        if (!System.IO.File.Exists(rutaFisica))
            return NotFound();

        var ext = Path.GetExtension(rutaFisica).ToLowerInvariant();
        var contentType = ext switch
        {
            ".png"  => "image/png",
            ".webp" => "image/webp",
            _       => "image/jpeg"
        };

        var bytes = await System.IO.File.ReadAllBytesAsync(rutaFisica);
        Response.Headers.CacheControl = "public, max-age=3600";
        return File(bytes, contentType);
    }
}
