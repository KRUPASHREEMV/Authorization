namespace TodoAuth.Application.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string otp);
    Task SendPasswordResetEmailAsync(string email, string otp);
}
