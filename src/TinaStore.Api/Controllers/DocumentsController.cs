using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly IPdfService _pdf;
    private readonly IExcelService _excel;

    public DocumentsController(IPdfService pdf, IExcelService excel)
    {
        _pdf = pdf;
        _excel = excel;
    }

    /// <summary>Genera y descarga el PDF de una factura.</summary>
    [HttpGet("facturas/{id:int}/pdf")]
    public async Task<IActionResult> InvoicePdf(int id)
    {
        try
        {
            var bytes = await _pdf.GenerateInvoicePdfAsync(id);
            return File(bytes, "application/pdf", $"factura-{id}.pdf");
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>Exporta todos los productos activos a Excel.</summary>
    [HttpGet("productos/excel")]
    public async Task<IActionResult> ExportProducts()
    {
        var bytes = await _excel.ExportProductsAsync();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"productos-{DateTime.Now:yyyyMMdd}.xlsx");
    }

    /// <summary>Descarga la plantilla Excel para carga masiva de productos.</summary>
    [HttpGet("productos/plantilla")]
    public async Task<IActionResult> ProductTemplate()
    {
        var bytes = await _excel.GetProductTemplateAsync();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "plantilla-productos.xlsx");
    }

    /// <summary>Importa productos desde un archivo Excel (multipart/form-data, campo: file).</summary>
    [HttpPost("productos/importar")]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { mensaje = "Debes enviar un archivo Excel." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
            return BadRequest(new { mensaje = "El archivo debe ser .xlsx o .xls." });

        await using var stream = file.OpenReadStream();
        var resultado = await _excel.ImportProductsAsync(stream);
        return Ok(resultado);
    }
}
