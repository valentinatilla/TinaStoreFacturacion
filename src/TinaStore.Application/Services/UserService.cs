using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Exceptions;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IAppPasswordHasher _hasher;

    public UserService(IUserRepository users, IAppPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<IEnumerable<UserInfoDto>> GetAllAsync()
    {
        var list = await _users.GetAllUsersAsync();
        return list.Select(MapToInfo);
    }

    public async Task<UserInfoDto?> GetByIdAsync(int id)
    {
        var user = await _users.GetByIdAsync(id);
        return user is null ? null : MapToInfo(user);
    }

    public async Task<UserInfoDto> CreateAsync(CreateUserDto dto)
    {
        if (await _users.EmailExistsAsync(dto.Email.Trim().ToLower()))
            throw new DomainException($"Ya existe un usuario con el correo '{dto.Email}'.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Role = Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var r) ? r : UserRole.Seller,
            IsActive = true,
            PasswordHash = string.Empty
        };
        user.PasswordHash = _hasher.Hash(dto.Password);

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();
        return MapToInfo(user);
    }

    public async Task<UserInfoDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null) return null;

        var emailChanged = !string.Equals(user.Email, dto.Email.Trim().ToLower(), StringComparison.OrdinalIgnoreCase);
        if (emailChanged && await _users.EmailExistsAsync(dto.Email.Trim().ToLower()))
            throw new DomainException($"Ya existe un usuario con el correo '{dto.Email}'.");

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLower();
        user.Role = Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var r) ? r : user.Role;
        user.IsActive = dto.IsActive;

        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();
        return MapToInfo(user);
    }

    public async Task<bool> ResetPasswordAsync(int id, ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new DomainException("Las contrasenas no coinciden.");

        var user = await _users.GetByIdAsync(id);
        if (user is null) return false;

        user.PasswordHash = _hasher.Hash(dto.NewPassword);
        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null) return false;

        user.IsActive = false;
        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();
        return true;
    }

    private static UserInfoDto MapToInfo(User u) => new(
        u.Id, u.FullName, u.Email, u.Role.ToString(), u.IsActive, u.LastLoginAt);
}
