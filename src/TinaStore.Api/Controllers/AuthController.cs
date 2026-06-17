using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;

namespace TinaStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Inicia sesión y retorna un token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Correo y contraseña son requeridos." });

        var result = await _auth.LoginAsync(dto);
        if (result is null)
            return Unauthorized(new { message = "Credenciales incorrectas o usuario inactivo." });

        return Ok(result);
    }

    /// <summary>Retorna el perfil del usuario autenticado.</summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var profile = await _auth.GetProfileAsync(userId);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Cambia la contraseña del usuario autenticado.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            return BadRequest(new { message = "Las contraseñas nuevas no coinciden." });

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var ok = await _auth.ChangePasswordAsync(userId, dto);
        return ok
            ? Ok(new { message = "Contraseña actualizada correctamente." })
            : BadRequest(new { message = "Contraseña actual incorrecta." });
    }
}
