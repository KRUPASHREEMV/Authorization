using TodoAuth.Domain.Entities;

namespace TodoAuth.Application.Services;

/// <summary>Abstraction over ASP.NET Identity's PasswordHasher to avoid a direct dependency in Application layer.</summary>
public interface IPasswordHasherService
{
    string HashPassword(ApplicationUser user, string password);
    bool VerifyPassword(ApplicationUser user, string hashedPassword, string providedPassword);
}
