using TinaStore.Domain.Enums;

namespace TinaStore.Application.DTOs;

// ─── Login ────────────────────────────────────────────────────────────────────

public record LoginDto(string Email, string Password);

public record TokenResponseDto(
    string AccessToken,
    string TokenType,
    int ExpiresInMinutes,
    UserInfoDto User);

// ─── Usuarios ─────────────────────────────────────────────────────────────────

public record UserInfoDto(
    int Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt);

public record CreateUserDto(
    string FullName,
    string Email,
    string Password,
    UserRole Role);

public record UpdateUserDto(
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);

public record ResetPasswordDto(
    string NewPassword,
    string ConfirmNewPassword);

/// <summary>Payload que la Web envía a la API para autenticar vía Google.</summary>
public record GoogleLoginDto(string IdToken);

/// <summary>Datos extraídos del id_token de Google después de validarlo en la capa API.</summary>
public record GoogleUserInfoDto(string Email, string FullName);
