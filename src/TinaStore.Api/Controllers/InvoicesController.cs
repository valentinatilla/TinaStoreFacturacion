using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Exceptions;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly IValidator<CreateInvoiceDto> _createValidator;
    private readonly IValidator<RegisterPaymentDto> _paymentValidator;
    private readonly IValidator<CancelInvoiceDto> _cancelValidator;
    private readonly IExcelService _excel;

    public InvoicesController(
        IInvoiceService service,
        IValidator<CreateInvoiceDto> createValidator,
        IValidator<RegisterPaymentDto> paymentValidator,
        IValidator<CancelInvoiceDto> cancelValidator,
        IExcelService excel)
    {
        _service = service;
        _createValidator = createValidator;
        _paymentValidator = paymentValidator;
        _cancelValidator = cancelValidator;
        _excel = excel;
    }

    /// <summary>Obtiene todas las facturas.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Obtiene facturas de un cliente específico.</summary>
    [HttpGet("cliente/{customerId:int}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
        => Ok(await _service.GetByCustomerAsync(customerId));

    /// <summary>Obtiene facturas en un rango de fechas. Formato: yyyy-MM-dd.</summary>
    [HttpGet("rango")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetByDateRangeAsync(from, to));

    /// <summary>Obtiene el detalle completo de una factura.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var factura = await _service.GetByIdAsync(id);
        return factura is null ? NotFound(new { mensaje = $"Factura {id} no encontrada." }) : Ok(factura);
    }

    /// <summary>Crea una nueva factura de venta.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        try
        {
            var factura = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = factura.Id }, factura);
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>Registra un pago o abono sobre una factura existente.</summary>
    [HttpPost("{id:int}/pagos")]
    public async Task<IActionResult> RegisterPayment(int id, [FromBody] RegisterPaymentDto dto)
    {
        var validation = await _paymentValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        try
        {
            var factura = await _service.RegisterPaymentAsync(id, dto);
            return factura is null ? NotFound(new { mensaje = $"Factura {id} no encontrada." }) : Ok(factura);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Anula una factura y revierte el stock.</summary>
    [HttpPost("{id:int}/anular")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelInvoiceDto dto)
    {
        var validation = await _cancelValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        try
        {
            var factura = await _service.CancelAsync(id, dto);
            return factura is null ? NotFound(new { mensaje = $"Factura {id} no encontrada." }) : Ok(factura);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Exporta las ventas a Excel con hoja de resumen y detalle de productos. Filtro opcional por rango de fechas.</summary>
    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var bytes = await _excel.ExportInvoicesAsync(desde, hasta);
        var nombre = desde.HasValue || hasta.HasValue
            ? $"ventas-{(desde?.ToString("yyyyMMdd") ?? "inicio")}-{(hasta?.ToString("yyyyMMdd") ?? "hoy")}.xlsx"
            : $"ventas-{DateTime.Now:yyyyMMdd}.xlsx";
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombre);
    }
}

