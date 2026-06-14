using System.Security.Cryptography;
using System.Text;
using TodoAuth.Application.DTOs;
using TodoAuth.Application.Exceptions;
using TodoAuth.Application.Services;

namespace TodoAuth.Application.Commands;

public record VerifyResetOtpCommand(string Email, string OTP);

public class VerifyResetOtpCommandHandler
{
    private readonly IOTPService _otpService;
    private readonly IUserRepository _userRepo;

    public VerifyResetOtpCommandHandler(IOTPService otpService, IUserRepository userRepo)
    {
        _otpService = otpService;
        _userRepo = userRepo;
    }

    public async Task<VerifyResetOtpResult> Handle(VerifyResetOtpCommand cmd)
    {
        var isValid = await _otpService.VerifyOTPAsync(cmd.Email, cmd.OTP, "PasswordReset");
        if (!isValid)
            throw new AuthException("Invalid or expired OTP.");

        var user = await _userRepo.FindByEmailAsync(cmd.Email)
            ?? throw new AuthException("User not found.");

        // Issue short-lived reset token; hash before storing
        var plainResetToken = Guid.NewGuid().ToString("N");
        using (var sha256 = SHA256.Create())
        {
            var tokenBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainResetToken));
            user.PasswordResetToken = Convert.ToBase64String(tokenBytes);
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
        }
        await _userRepo.UpdateAsync(user);

        // Return plain token to Angular — stored in service state, NEVER placed in URL
        return new VerifyResetOtpResult(plainResetToken, "OTP verified. Proceed to set your new password.");
    }
}
