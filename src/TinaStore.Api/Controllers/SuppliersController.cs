using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;
    private readonly IValidator<CreateSupplierDto> _createValidator;
    private readonly IValidator<UpdateSupplierDto> _updateValidator;

    public SuppliersController(
        ISupplierService service,
        IValidator<CreateSupplierDto> createValidator,
        IValidator<UpdateSupplierDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Obtiene todos los proveedores. Parámetro opcional: soloActivos=true.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivos = false)
    {
        var proveedores = await _service.GetAllAsync(soloActivos);
        return Ok(proveedores);
    }

    /// <summary>Obtiene un proveedor por su ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var proveedor = await _service.GetByIdAsync(id);
        return proveedor is null ? NotFound(new { mensaje = $"Proveedor {id} no encontrado." }) : Ok(proveedor);
    }

    /// <summary>Busca proveedores por nombre, contacto o NIT.</summary>
    [HttpGet("buscar")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { mensaje = "El parámetro de búsqueda no puede estar vacío." });

        var resultados = await _service.SearchAsync(q);
        return Ok(resultados);
    }

    /// <summary>Crea un nuevo proveedor.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
    {
        var validacion = await _createValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>Actualiza los datos de un proveedor existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        var validacion = await _updateValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

        var actualizado = await _service.UpdateAsync(id, dto);
        return actualizado is null ? NotFound(new { mensaje = $"Proveedor {id} no encontrado." }) : Ok(actualizado);
    }

    /// <summary>Elimina (baja lógica) un proveedor.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _service.DeleteAsync(id);
        return eliminado ? NoContent() : NotFound(new { mensaje = $"Proveedor {id} no encontrado." });
    }
}
