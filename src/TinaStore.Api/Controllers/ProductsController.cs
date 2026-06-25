using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Exceptions;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private static readonly string[] _extensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
    private const long _tamañoMaximoBytes = 2 * 1024 * 1024; // 2 MB

    /// <summary>
    /// Firmas de bytes (magic bytes) de los formatos de imagen permitidos.
    /// Evita que se suban archivos maliciosos con extensión de imagen.
    /// </summary>
    private static readonly (byte[] Firma, string Descripcion)[] _firmasPermitidas =
    [
        (new byte[] { 0xFF, 0xD8, 0xFF },             "JPEG"),
        (new byte[] { 0x89, 0x50, 0x4E, 0x47 },       "PNG"),
        (new byte[] { 0x52, 0x49, 0x46, 0x46 },       "WEBP (RIFF)"),
    ];

    private static async Task<bool> EsImagenValidaAsync(IFormFile archivo)
    {
        var buffer = new byte[8];
        await using var stream = archivo.OpenReadStream();
        var leidos = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
        if (leidos < 4) return false;

        foreach (var (firma, _) in _firmasPermitidas)
        {
            if (buffer.Take(firma.Length).SequenceEqual(firma))
                return true;
        }
        return false;
    }

    private readonly IProductService _service;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;
    private readonly string _uploadsRoot;

    public ProductsController(
        IProductService service,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator,
        IWebHostEnvironment env,
        IOptions<UploadsSettings> uploadsOptions)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        var basePath = uploadsOptions.Value.BasePath;
        _uploadsRoot = string.IsNullOrWhiteSpace(basePath) ? env.WebRootPath : basePath;
    }

    /// <summary>Obtiene todos los productos. Parámetro opcional: soloActivos=true.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivos = false)
    {
        var productos = await _service.GetAllAsync(soloActivos);
        return Ok(productos);
    }

    /// <summary>Obtiene un producto completo por su ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var producto = await _service.GetByIdAsync(id);
        return producto is null ? NotFound(new { mensaje = $"Producto {id} no encontrado." }) : Ok(producto);
    }

    /// <summary>Obtiene productos con stock por debajo del mínimo.</summary>
    [HttpGet("stock-bajo")]
    public async Task<IActionResult> GetLowStock()
    {
        var productos = await _service.GetLowStockAsync();
        return Ok(productos);
    }

    /// <summary>Busca productos por nombre, código interno o SKU.</summary>
    [HttpGet("buscar")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { mensaje = "El parámetro de búsqueda no puede estar vacío." });

        var resultados = await _service.SearchAsync(q);
        return Ok(resultados);
    }

    /// <summary>Crea un nuevo producto en el inventario.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var validacion = await _createValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(new { mensaje = string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)) });

        try
        {
            var creado = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Actualiza los datos de un producto existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var validacion = await _updateValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(new { mensaje = string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)) });

        try
        {
            var actualizado = await _service.UpdateAsync(id, dto);
            return actualizado is null ? NotFound(new { mensaje = $"Producto {id} no encontrado." }) : Ok(actualizado);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Elimina (baja lógica) un producto del inventario.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _service.DeleteAsync(id);
        return eliminado ? NoContent() : NotFound(new { mensaje = $"Producto {id} no encontrado." });
    }

    /// <summary>Sube o reemplaza la imagen de un producto. Máximo 5 MB; JPG, PNG o WEBP.</summary>
    [HttpPost("{id:int}/imagen")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(int id, IFormFile archivo)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { mensaje = "No se recibió ningún archivo." });

        if (archivo.Length > _tamañoMaximoBytes)
            return BadRequest(new { mensaje = "La imagen supera el tamaño máximo de 2 MB." });

        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!_extensionesPermitidas.Contains(ext))
            return BadRequest(new { mensaje = "Solo se permiten imágenes JPG, PNG o WEBP." });

        // Validar el contenido real del archivo (magic bytes), no solo la extensión
        if (!await EsImagenValidaAsync(archivo))
            return BadRequest(new { mensaje = "El archivo no es una imagen válida (JPG, PNG o WEBP)." });

        var producto = await _service.GetByIdAsync(id);
        if (producto is null)
            return NotFound(new { mensaje = $"Producto {id} no encontrado." });

        var carpeta = Path.Combine(_uploadsRoot, "uploads", "productos");
        Directory.CreateDirectory(carpeta);

        // Borrar imagen anterior si existe
        if (!string.IsNullOrEmpty(producto.ImagePath))
        {
            var rutaAnterior = Path.Combine(_uploadsRoot, producto.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(rutaAnterior))
                System.IO.File.Delete(rutaAnterior);
        }

        var nombreArchivo = $"{id}_{Guid.NewGuid():N}{ext}";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);

        await using (var stream = System.IO.File.Create(rutaFisica))
            await archivo.CopyToAsync(stream);

        var rutaRelativa = $"/uploads/productos/{nombreArchivo}";
        var actualizado = await _service.UpdateImagePathAsync(id, rutaRelativa);

        return Ok(actualizado);
    }

    /// <summary>Realiza una entrada rápida de stock para un producto.</summary>
    [HttpPost("{id:int}/ajuste-stock")]
    public async Task<IActionResult> AjustarStock(int id, [FromBody] AjusteStockDto dto)
    {
        if (dto.Cantidad <= 0)
            return BadRequest(new { mensaje = "La cantidad debe ser mayor a cero." });

        var resultado = await _service.AjustarStockAsync(id, dto);
        return resultado is null
            ? NotFound(new { mensaje = $"Producto {id} no encontrado." })
            : Ok(resultado);
    }

    /// <summary>Actualiza costo, precio de venta y/o stock de varios productos en un solo lote.</summary>
    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdate([FromBody] List<BulkUpdateItemDto> items)
    {
        if (items is null || items.Count == 0)
            return BadRequest(new { mensaje = "La lista de productos a actualizar está vacía." });

        var resultado = await _service.BulkUpdateAsync(items);
        return Ok(resultado);
    }

    /// <summary>Elimina la imagen de un producto.</summary>
    [HttpDelete("{id:int}/imagen")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var producto = await _service.GetByIdAsync(id);
        if (producto is null)
            return NotFound(new { mensaje = $"Producto {id} no encontrado." });

        if (!string.IsNullOrEmpty(producto.ImagePath))
        {
            var rutaFisica = Path.Combine(_uploadsRoot, producto.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(rutaFisica))
                System.IO.File.Delete(rutaFisica);
        }

        await _service.UpdateImagePathAsync(id, null);
        return NoContent();
    }
}
