using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;
    private readonly IValidator<CreateCustomerDto> _createValidator;
    private readonly IValidator<UpdateCustomerDto> _updateValidator;

    public CustomersController(
        ICustomerService service,
        IValidator<CreateCustomerDto> createValidator,
        IValidator<UpdateCustomerDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Obtiene todos los clientes. Parámetro opcional: soloActivos=true.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivos = false)
    {
        var clientes = await _service.GetAllAsync(soloActivos);
        return Ok(clientes);
    }

    /// <summary>Obtiene un cliente por su ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cliente = await _service.GetByIdAsync(id);
        return cliente is null ? NotFound(new { mensaje = $"Cliente {id} no encontrado." }) : Ok(cliente);
    }

    /// <summary>Busca clientes por nombre, documento o teléfono.</summary>
    [HttpGet("buscar")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { mensaje = "El parámetro de búsqueda no puede estar vacío." });

        var resultados = await _service.SearchAsync(q);
        return Ok(resultados);
    }

    /// <summary>Crea un nuevo cliente.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var validacion = await _createValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>Actualiza los datos de un cliente existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        var validacion = await _updateValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

        var actualizado = await _service.UpdateAsync(id, dto);
        return actualizado is null ? NotFound(new { mensaje = $"Cliente {id} no encontrado." }) : Ok(actualizado);
    }

    /// <summary>Elimina (baja lógica) un cliente.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _service.DeleteAsync(id);
        return eliminado ? NoContent() : NotFound(new { mensaje = $"Cliente {id} no encontrado." });
    }
}
