using System.Security.Claims;
using Google.Apis.Auth;
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
    private readonly IConfiguration _config;

    public AuthController(IAuthService auth, IConfiguration config)
    {
        _auth = auth;
        _config = config;
    }

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

    /// <summary>
    /// Valida un id_token de Google, verifica que el correo esté en la lista de permitidos
    /// y devuelve un JWT de TinaStore.
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IdToken))
            return BadRequest(new { message = "El id_token es requerido." });

        var clientId = _config["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return StatusCode(501, new { message = "El inicio de sesión con Google no está configurado en este servidor." });

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, validationSettings);
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { message = "Token de Google inválido o expirado." });
        }

        // Verificar que el correo esté en la lista de correos autorizados.
        // Se leen tanto arrays JSON como entradas individuales de user-secrets (Google:AllowedEmails:0, :1, ...)
        var allowedSection = _config.GetSection("Google:AllowedEmails");
        var allowedEmails  = allowedSection.Get<string[]>()
                          ?? allowedSection.GetChildren().Select(c => c.Value ?? "").ToArray();

        // Lista vacía = acceso denegado. Google activo sin lista es un error de configuración.
        if (allowedEmails.Length == 0)
            return Unauthorized(new { message = "El acceso con Google no está habilitado en este servidor. Contacta al administrador." });

        if (!allowedEmails.Contains(payload.Email, StringComparer.OrdinalIgnoreCase))
            return Unauthorized(new { message = "Este correo de Google no está autorizado para acceder al sistema." });

        var result = await _auth.LoginWithGoogleAsync(new GoogleUserInfoDto(payload.Email, payload.Name));
        if (result is null)
            return Unauthorized(new { message = "Usuario inactivo o no autorizado." });

        return Ok(result);
    }
}
