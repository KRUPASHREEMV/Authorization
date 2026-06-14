using TodoAuth.Application.DTOs;
using TodoAuth.Application.Exceptions;
using TodoAuth.Application.Services;

namespace TodoAuth.Application.Commands;

public record VerifyOTPCommand(string Email, string OTP);

public class VerifyOTPCommandHandler
{
    private readonly IOTPService _otpService;
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenGenerator _jwtGen;

    public VerifyOTPCommandHandler(IOTPService otpService, IUserRepository userRepo, IJwtTokenGenerator jwtGen)
    {
        _otpService = otpService;
        _userRepo = userRepo;
        _jwtGen = jwtGen;
    }

    public async Task<(AuthResult Result, string PlainRefreshToken)> Handle(VerifyOTPCommand cmd)
    {
        var isValid = await _otpService.VerifyOTPAsync(cmd.Email, cmd.OTP, "Registration");
        if (!isValid)
            throw new AuthException("Invalid or expired OTP.");

        var user = await _userRepo.FindByEmailAsync(cmd.Email)
            ?? throw new AuthException("User not found.");

        user.IsVerified = true;
        user.VerifiedAt = DateTime.UtcNow;

        var roles = await _userRepo.GetRolesAsync(user);
        var token = _jwtGen.GenerateToken(user, roles);
        var plainRefreshToken = _jwtGen.GenerateRefreshToken();

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainRefreshToken));
        user.RefreshToken = Convert.ToBase64String(hashBytes);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(60);

        await _userRepo.UpdateAsync(user);

        return (new AuthResult(token, user.Email!, roles.ToList()), plainRefreshToken);
    }
}
