namespace TodoAuth.Application.Services;

public interface IOTPService
{
    /// <summary>Generates a CSPRNG OTP, hashes it, stores the hash, emails the plaintext OTP. Returns plaintext OTP.</summary>
    Task<string> GenerateOTPAsync(string email, string purpose);

    /// <summary>Hashes the incoming OTP and compares with stored hash.</summary>
    Task<bool> VerifyOTPAsync(string email, string otp, string purpose);

    Task<bool> IsOTPExpiredAsync(string email, string purpose);

    /// <summary>Returns true if the email has made >= 3 OTP requests in the last hour.</summary>
    Task<bool> IsRateLimitedAsync(string email);
}
