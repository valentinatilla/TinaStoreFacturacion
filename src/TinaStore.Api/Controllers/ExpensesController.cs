using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;
    private readonly IValidator<CreateExpenseDto> _createValidator;
    private readonly IValidator<UpdateExpenseDto> _updateValidator;

    public ExpensesController(
        IExpenseService service,
        IValidator<CreateExpenseDto> createValidator,
        IValidator<UpdateExpenseDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Obtiene todos los egresos.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Obtiene egresos por rango de fechas.</summary>
    [HttpGet("rango")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetByDateRangeAsync(from, to));

    /// <summary>Obtiene egresos de una categoría específica.</summary>
    [HttpGet("categoria/{categoryId:int}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
        => Ok(await _service.GetByCategoryAsync(categoryId));

    /// <summary>Obtiene un egreso por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var gasto = await _service.GetByIdAsync(id);
        return gasto is null ? NotFound(new { mensaje = $"Egreso {id} no encontrado." }) : Ok(gasto);
    }

    /// <summary>Registra un nuevo egreso.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var gasto = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = gasto.Id }, gasto);
    }

    /// <summary>Actualiza un egreso existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var gasto = await _service.UpdateAsync(id, dto);
        return gasto is null ? NotFound(new { mensaje = $"Egreso {id} no encontrado." }) : Ok(gasto);
    }

    /// <summary>Anula un egreso (borrado lógico).</summary>
    [HttpPost("{id:int}/anular")]
    public async Task<IActionResult> Cancel(int id)
    {
        var ok = await _service.CancelAsync(id);
        return ok ? NoContent() : NotFound(new { mensaje = $"Egreso {id} no encontrado." });
    }
}
