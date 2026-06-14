using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TodoAuth.Application.Services;
using TodoAuth.Domain.Entities;

namespace TodoAuth.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config) => _config = config;

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        // SECURITY: Load JWT secret from environment variable; dev fallback from appsettings with warning
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        if (string.IsNullOrEmpty(jwtSecret))
        {
            jwtSecret = _config["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required.");
            Console.WriteLine("⚠️  WARNING: Using JWT secret from appsettings.json. Set JWT_SECRET_KEY env var in production!");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(360), // 6 hours
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Returns a plain Guid. Caller must SHA-256 hash it before DB storage; plain token goes into HttpOnly cookie.</summary>
    public string GenerateRefreshToken() => Guid.NewGuid().ToString();

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? _config["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT_SECRET_KEY is required.");

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _config["Jwt:Audience"],
            ValidateLifetime = false // expired is OK here
        };

        return new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
    }
}
