using System.Security.Cryptography;
using System.Text;
using TodoAuth.Application.DTOs;
using TodoAuth.Application.Exceptions;
using TodoAuth.Application.Services;

namespace TodoAuth.Application.Commands;

public record RefreshTokenCommand(string PlainRefreshToken);

public class RefreshTokenCommandHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenGenerator _jwtGen;

    public RefreshTokenCommandHandler(IUserRepository userRepo, IJwtTokenGenerator jwtGen)
    {
        _userRepo = userRepo;
        _jwtGen = jwtGen;
    }

    public async Task<(AuthResult Result, string NewPlainRefreshToken)> Handle(RefreshTokenCommand cmd)
    {
        // Hash incoming plain refresh token and find matching user
        using var sha256 = SHA256.Create();
        var incomingHash = Convert.ToBase64String(
            sha256.ComputeHash(Encoding.UTF8.GetBytes(cmd.PlainRefreshToken)));

        // Find user by refresh token hash — scan all users (small table, acceptable)
        // In production, consider indexing RefreshToken or storing email in cookie
        var allUsers = await _userRepo.GetAllAsync();
        var user = allUsers.FirstOrDefault(u => u.RefreshToken == incomingHash);

        if (user == null)
            throw new AuthException("Invalid refresh token.");

        if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new AuthException("Refresh token expired. Please log in again.");

        var roles = await _userRepo.GetRolesAsync(user);
        var newToken = _jwtGen.GenerateToken(user, roles);
        var newPlainRefreshToken = _jwtGen.GenerateRefreshToken();

        // Rotate: invalidate old, store new hash
        var newHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(newPlainRefreshToken));
        user.RefreshToken = Convert.ToBase64String(newHashBytes);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(60);
        await _userRepo.UpdateAsync(user);

        return (new AuthResult(newToken, user.Email!, roles.ToList()), newPlainRefreshToken);
    }
}
