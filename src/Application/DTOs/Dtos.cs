namespace TodoAuth.Application.DTOs;

// Refresh token is NOT included — it arrives as HttpOnly Secure Cookie via Set-Cookie header
public record AuthResult(string Token, string Email, List<string> Roles);

public record RegistrationResult(string Message, bool RequiresVerification);

public record PasswordResetResult(string Message, bool RequiresOTPVerification);

// resetToken is returned in body so Angular stores it in service state (NEVER in URL)
public record VerifyResetOtpResult(string ResetToken, string Message);

public record ResendOTPResult(string Message);

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

public record LoginRequest(string Email, string Password);
