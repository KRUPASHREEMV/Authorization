using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoAuth.Application.Services;
using TodoAuth.Domain.Data;
using TodoAuth.Domain.Entities;

namespace TodoAuth.Infrastructure.Services;

public class OTPService : IOTPService
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<OTPService> _logger;

    public OTPService(IEmailService emailService, ApplicationDbContext dbContext, ILogger<OTPService> logger)
    {
        _emailService = emailService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string> GenerateOTPAsync(string email, string purpose)
    {
        // SECURITY: Cryptographically secure OTP — NOT new Random()
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        // Remove any existing OTP for this email+purpose
        var existing = await _dbContext.EmailVerifications
            .Where(e => e.Email == email && e.Purpose == purpose)
            .ToListAsync();

        int requestCount = existing.Select(e => e.RequestCount).FirstOrDefault() + 1;
        _dbContext.EmailVerifications.RemoveRange(existing);

        // SECURITY: Hash OTP before storing — never store plaintext in DB
        using var sha256 = SHA256.Create();
        var otpHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(otp)));

        var verification = new EmailVerification
        {
            Id = Guid.NewGuid(),
            Email = email,
            OTPHash = otpHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsVerified = false,
            Purpose = purpose,
            RequestCount = requestCount,
            LastRequestAt = DateTime.UtcNow
        };

        _dbContext.EmailVerifications.Add(verification);
        await _dbContext.SaveChangesAsync();

        // Send plaintext OTP in email
        if (purpose == "Registration")
            await _emailService.SendVerificationEmailAsync(email, otp);
        else
            await _emailService.SendPasswordResetEmailAsync(email, otp);

        _logger.LogInformation("OTP generated for {Email}, purpose={Purpose}", email, purpose);
        return otp;
    }

    public async Task<bool> VerifyOTPAsync(string email, string otp, string purpose)
    {
        // SECURITY: Hash incoming OTP, compare with stored hash
        using var sha256 = SHA256.Create();
        var otpHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(otp)));

        var verification = await _dbContext.EmailVerifications
            .FirstOrDefaultAsync(e => e.Email == email && e.OTPHash == otpHash && e.Purpose == purpose);

        if (verification == null)
            return false;

        if (verification.ExpiresAt < DateTime.UtcNow)
        {
            _dbContext.EmailVerifications.Remove(verification);
            await _dbContext.SaveChangesAsync();
            return false;
        }

        verification.IsVerified = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsOTPExpiredAsync(string email, string purpose)
    {
        var verification = await _dbContext.EmailVerifications
            .FirstOrDefaultAsync(e => e.Email == email && e.Purpose == purpose);
        return verification == null || verification.ExpiresAt < DateTime.UtcNow;
    }

    public async Task<bool> IsRateLimitedAsync(string email)
    {
        var cutoff = DateTime.UtcNow.AddHours(-1);
        var count = await _dbContext.EmailVerifications
            .CountAsync(e => e.Email == email && e.LastRequestAt > cutoff);
        return count >= 3;
    }
}
