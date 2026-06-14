using TodoAuth.Application.DTOs;
using TodoAuth.Application.Exceptions;
using TodoAuth.Application.Services;

namespace TodoAuth.Application.Commands;

public record AuthenticateUserCommand(string Email, string Password);

public class AuthenticateUserCommandHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenGenerator _jwtGen;

    public AuthenticateUserCommandHandler(IUserRepository userRepo, IJwtTokenGenerator jwtGen)
    {
        _userRepo = userRepo;
        _jwtGen = jwtGen;
    }

    public async Task<(AuthResult Result, string PlainRefreshToken)> Handle(AuthenticateUserCommand cmd)
    {
        var user = await _userRepo.FindByEmailAsync(cmd.Email)
            ?? throw new AuthException("Invalid email or password.");

        if (!user.IsVerified)
            throw new AuthException("Please verify your email before logging in.");

        // Password validation is done via UserManager in the handler caller (Infrastructure)
        var roles = await _userRepo.GetRolesAsync(user);
        var token = _jwtGen.GenerateToken(user, roles);
        var plainRefreshToken = _jwtGen.GenerateRefreshToken();

        // Hash and store refresh token
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainRefreshToken));
        user.RefreshToken = Convert.ToBase64String(hashBytes);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(60);
        await _userRepo.UpdateAsync(user);

        return (new AuthResult(token, user.Email!, roles.ToList()), plainRefreshToken);
    }
}
