using System.Security.Cryptography;
using System.Text;
using TodoAuth.Application.DTOs;
using TodoAuth.Application.Exceptions;
using TodoAuth.Application.Services;
using TodoAuth.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace TodoAuth.Application.Commands;

public record ResetPasswordCommand(string Email, string ResetToken, string NewPassword);

public class ResetPasswordCommandHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ApplicationDbContext _dbContext;

    public ResetPasswordCommandHandler(
        IUserRepository userRepo,
        IPasswordHasherService passwordHasher,
        ApplicationDbContext dbContext)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
    }

    public async Task<PasswordResetResult> Handle(ResetPasswordCommand cmd)
    {
        var user = await _userRepo.FindByEmailAsync(cmd.Email)
            ?? throw new AuthException("User not found.");

        // Hash incoming reset token and compare with stored hash
        using (var sha256 = SHA256.Create())
        {
            var tokenBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cmd.ResetToken));
            var tokenHash = Convert.ToBase64String(tokenBytes);

            if (user.PasswordResetToken != tokenHash)
                throw new AuthException("Invalid reset token.");

            if (user.PasswordResetTokenExpiresAt == null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
                throw new AuthException("Reset token expired. Please restart the password reset flow.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, cmd.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        await _userRepo.UpdateAsync(user);

        // Delete the used OTP record
        var otpRecord = await _dbContext.EmailVerifications
            .FirstOrDefaultAsync(e => e.Email == cmd.Email && e.Purpose == "PasswordReset");
        if (otpRecord != null)
            _dbContext.EmailVerifications.Remove(otpRecord);
        await _dbContext.SaveChangesAsync();

        return new PasswordResetResult("Password reset successful! You can now log in with your new password.", false);
    }
}
