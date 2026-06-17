using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductsController(
        IProductService service,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>Actualiza los datos de un producto existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var validacion = await _updateValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

        var actualizado = await _service.UpdateAsync(id, dto);
        return actualizado is null ? NotFound(new { mensaje = $"Producto {id} no encontrado." }) : Ok(actualizado);
    }

    /// <summary>Elimina (baja lógica) un producto del inventario.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _service.DeleteAsync(id);
        return eliminado ? NoContent() : NotFound(new { mensaje = $"Producto {id} no encontrado." });
    }
}
