using System.Security.Claims;
using TodoAuth.Domain.Entities;

namespace TodoAuth.Application.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IList<string> roles);

    /// <summary>Returns a plain Guid string. Caller must SHA-256 hash it before storing in DB.</summary>
    string GenerateRefreshToken();

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
