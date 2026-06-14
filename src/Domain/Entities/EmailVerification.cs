namespace TodoAuth.Domain.Entities;

public class EmailVerification
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;

    // SHA-256 hash of plaintext OTP — never store OTP in plaintext
    public string OTPHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsVerified { get; set; } = false;

    // "Registration" or "PasswordReset"
    public string Purpose { get; set; } = string.Empty;

    public int RequestCount { get; set; } = 1;
    public DateTime LastRequestAt { get; set; } = DateTime.UtcNow;
}
