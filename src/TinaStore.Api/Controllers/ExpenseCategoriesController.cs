using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ExpenseCategoriesController : ControllerBase
{
    private readonly IExpenseCategoryService _service;
    private readonly IValidator<CreateExpenseCategoryDto> _createValidator;
    private readonly IValidator<UpdateExpenseCategoryDto> _updateValidator;

    public ExpenseCategoriesController(
        IExpenseCategoryService service,
        IValidator<CreateExpenseCategoryDto> createValidator,
        IValidator<UpdateExpenseCategoryDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Obtiene todas las categorías de gasto.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivas = false)
        => Ok(await _service.GetAllAsync(soloActivas));

    /// <summary>Obtiene una categoría de gasto por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _service.GetByIdAsync(id);
        return categoria is null ? NotFound(new { mensaje = $"Categoría {id} no encontrada." }) : Ok(categoria);
    }

    /// <summary>Crea una nueva categoría de gasto.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseCategoryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var categoria = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    /// <summary>Actualiza una categoría de gasto.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseCategoryDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var categoria = await _service.UpdateAsync(id, dto);
        return categoria is null ? NotFound(new { mensaje = $"Categoría {id} no encontrada." }) : Ok(categoria);
    }

    /// <summary>Elimina (borrado lógico) una categoría de gasto.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound(new { mensaje = $"Categoría {id} no encontrada." });
    }
}
