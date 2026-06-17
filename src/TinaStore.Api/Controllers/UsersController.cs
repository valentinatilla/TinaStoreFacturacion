using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Exceptions;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    /// <summary>Obtiene todos los usuarios del sistema.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetAllAsync();
        return Ok(users);
    }

    /// <summary>Obtiene un usuario por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Crea un nuevo usuario.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Correo y contraseña son requeridos." });

        try
        {
            var user = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (DomainException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Actualiza datos de un usuario.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _service.UpdateAsync(id, dto);
            return user is null ? NotFound() : Ok(user);
        }
        catch (DomainException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Restablece la contraseña de un usuario (solo Admin).</summary>
    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        try
        {
            var ok = await _service.ResetPasswordAsync(id, dto);
            return ok
                ? Ok(new { message = "Contraseña restablecida correctamente." })
                : NotFound();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Desactiva un usuario (borrado lógico).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
