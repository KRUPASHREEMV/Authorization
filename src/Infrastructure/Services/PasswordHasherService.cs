using Microsoft.AspNetCore.Identity;
using TodoAuth.Application.Services;
using TodoAuth.Domain.Entities;

namespace TodoAuth.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly IPasswordHasher<ApplicationUser> _hasher;

    public PasswordHasherService(IPasswordHasher<ApplicationUser> hasher) => _hasher = hasher;

    public string HashPassword(ApplicationUser user, string password) =>
        _hasher.HashPassword(user, password);

    public bool VerifyPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
