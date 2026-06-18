using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IAppPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository users,
        IAppPasswordHasher hasher,
        ITokenService tokenService)
    {
        _users = users;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (user is null || !user.IsActive)
            return null;

        if (!_hasher.Verify(user.PasswordHash, dto.Password))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();

        var token = _tokenService.GenerateToken(
            user.Id, user.Email, user.FullName, user.Role.ToString());

        return new TokenResponseDto(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresInMinutes: _tokenService.ExpiresInMinutes,
            User: MapToInfo(user));
    }

    public async Task<UserInfoDto?> GetProfileAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId);
        return user is null ? null : MapToInfo(user);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            return false;

        var user = await _users.GetByIdAsync(userId);
        if (user is null) return false;

        if (!_hasher.Verify(user.PasswordHash, dto.CurrentPassword))
            return false;

        user.PasswordHash = _hasher.Hash(dto.NewPassword);
        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();
        return true;
    }

    public async Task<TokenResponseDto?> LoginWithGoogleAsync(GoogleUserInfoDto googleUser)
    {
        var email = googleUser.Email.Trim().ToLower();
        var user = await _users.GetByEmailAsync(email);

        if (user is null)
        {
            // Crear usuario automáticamente con contraseña aleatoria (no usable para login con password)
            user = new User
            {
                FullName = googleUser.FullName,
                Email = email,
                Role = Domain.Enums.UserRole.Admin,
                IsActive = true,
                PasswordHash = _hasher.Hash(Guid.NewGuid().ToString())
            };
            await _users.AddAsync(user);
            await _users.SaveChangesAsync();
        }
        else if (!user.IsActive)
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();

        var token = _tokenService.GenerateToken(
            user.Id, user.Email, user.FullName, user.Role.ToString());

        return new TokenResponseDto(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresInMinutes: _tokenService.ExpiresInMinutes,
            User: MapToInfo(user));
    }

    private static UserInfoDto MapToInfo(User u) => new(
        u.Id, u.FullName, u.Email, u.Role.ToString(), u.IsActive, u.LastLoginAt);

    public async Task<TokenResponseDto?> LoginWithGoogleAsync(GoogleUserInfoDto googleUser)
    {
        var email = googleUser.Email.Trim().ToLower();
        var user = await _users.GetByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                FullName = googleUser.FullName,
                Email = email,
                Role = Domain.Enums.UserRole.Admin,
                IsActive = true,
                PasswordHash = _hasher.Hash(Guid.NewGuid().ToString())
            };
            await _users.AddAsync(user);
            await _users.SaveChangesAsync();
        }
        else if (!user.IsActive)
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();

        var token = _tokenService.GenerateToken(
            user.Id, user.Email, user.FullName, user.Role.ToString());

        return new TokenResponseDto(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresInMinutes: _tokenService.ExpiresInMinutes,
            User: MapToInfo(user));
    }
}
