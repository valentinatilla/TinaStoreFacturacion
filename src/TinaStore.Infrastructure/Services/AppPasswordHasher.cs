using Microsoft.AspNetCore.Identity;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;

namespace TinaStore.Infrastructure.Services;

public class AppPasswordHasher : IAppPasswordHasher
{
    private readonly IPasswordHasher<User> _inner;

    public AppPasswordHasher(IPasswordHasher<User> inner) => _inner = inner;

    public string Hash(string password)
    {
        var dummy = new User();
        return _inner.HashPassword(dummy, password);
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        var dummy = new User();
        var result = _inner.VerifyHashedPassword(dummy, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
