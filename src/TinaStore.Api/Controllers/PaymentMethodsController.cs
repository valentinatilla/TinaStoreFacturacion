using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _service;
    private readonly IValidator<CreatePaymentMethodDto> _createValidator;
    private readonly IValidator<UpdatePaymentMethodDto> _updateValidator;

    public PaymentMethodsController(
        IPaymentMethodService service,
        IValidator<CreatePaymentMethodDto> createValidator,
        IValidator<UpdatePaymentMethodDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Obtiene todos los métodos de pago. Parámetro opcional: soloActivos=true.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivos = false)
    {
        var metodos = await _service.GetAllAsync(soloActivos);
        return Ok(metodos);
    }

    /// <summary>Obtiene un método de pago por su ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var metodo = await _service.GetByIdAsync(id);
        return metodo is null ? NotFound(new { mensaje = $"Método de pago {id} no encontrado." }) : Ok(metodo);
    }

    /// <summary>Crea un nuevo método de pago.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentMethodDto dto)
    {
        var validacion = await _createValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>Actualiza un método de pago existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentMethodDto dto)
    {
        var validacion = await _updateValidator.ValidateAsync(dto);
        if (!validacion.IsValid)
            return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

        var actualizado = await _service.UpdateAsync(id, dto);
        return actualizado is null ? NotFound(new { mensaje = $"Método de pago {id} no encontrado." }) : Ok(actualizado);
    }

    /// <summary>Elimina (baja lógica) un método de pago.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _service.DeleteAsync(id);
        return eliminado ? NoContent() : NotFound(new { mensaje = $"Método de pago {id} no encontrado." });
    }
}
