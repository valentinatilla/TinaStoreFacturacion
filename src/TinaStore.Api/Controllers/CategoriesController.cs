using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updateValidator;

    public CategoriesController(
        ICategoryService service,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<UpdateCategoryDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Obtiene todas las categorías. Parámetro opcional: soloActivas=true.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivas = false)
    {
        var categorias = await _service.GetAllAsync(soloActivas);
        return Ok(categorias);
    }

    /// <summary>Obtiene una categoría por su ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _service.GetByIdAsync(id);
        return categoria is null ? NotFound(new { mensaje = $"Categoría {id} no encontrada." }) : Ok(categoria);
    }

    /// <summary>Crea una nueva categoría.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var validacion = await _createValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(new { mensaje = string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)) });

        try
        {
            var creada = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>Actualiza una categoría existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var validacion = await _updateValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(new { mensaje = string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)) });

        try
        {
            var actualizada = await _service.UpdateAsync(id, dto);
            return actualizada is null
                ? NotFound(new { mensaje = $"Categoría {id} no encontrada." })
                : Ok(actualizada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Elimina (baja lógica) una categoría. Si tiene productos, los reasigna a "Sin categoría".</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var eliminada = await _service.DeleteAsync(id);
            return eliminada ? NoContent() : NotFound(new { mensaje = $"Categoría {id} no encontrada." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
