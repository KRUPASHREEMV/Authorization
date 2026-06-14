using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TodoAuth.Application.Services;

namespace TodoAuth.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string email, string otp) =>
        SendEmailAsync(email, "Verify Your TodoAuth Email", otp, "Email Verification");

    public Task SendPasswordResetEmailAsync(string email, string otp) =>
        SendEmailAsync(email, "Reset Your TodoAuth Password", otp, "Password Reset");

    private async Task SendEmailAsync(string email, string subject, string otp, string purpose)
    {
        var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Segoe UI, Arial, sans-serif; background:#0b0d14; color:#e2e8f0; max-width:600px; margin:0 auto; padding:20px;'>
  <div style='background:linear-gradient(135deg,#131620,#1c2030); border:1px solid #2a2f4a; border-radius:12px; padding:32px;'>
    <h1 style='font-size:1.4rem; font-weight:800; background:linear-gradient(135deg,#4f8ef7,#7c5cfc); -webkit-background-clip:text; -webkit-text-fill-color:transparent; margin-bottom:8px;'>TodoAuth</h1>
    <p style='color:#94a3b8; font-size:0.85rem; margin-bottom:24px;'>{purpose}</p>
    <p style='font-size:0.9rem; margin-bottom:16px;'>Your one-time code is:</p>
    <div style='background:#242840; border:2px solid #4f8ef7; border-radius:10px; padding:20px; text-align:center; margin-bottom:24px;'>
      <span style='font-size:2.5rem; font-weight:900; color:#4f8ef7; letter-spacing:0.5em; font-family:monospace;'>{otp}</span>
    </div>
    <p style='color:#94a3b8; font-size:0.78rem;'>⏰ This code expires in <strong>10 minutes</strong>.</p>
    <p style='color:#64748b; font-size:0.72rem; margin-top:16px;'>If you did not request this, please ignore this email.</p>
  </div>
</body>
</html>";

        try
        {
            using var client = new SmtpClient(_config["Email:SmtpServer"])
            {
                Port = int.Parse(_config["Email:SmtpPort"] ?? "587"),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["Email:Username"],
                    _config["Email:Password"])
            };

            var msg = new MailMessage
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                From = new MailAddress(
                    _config["Email:FromAddress"] ?? _config["Email:Username"] ?? "noreply@todoauth.com",
                    _config["Email:FromName"] ?? "TodoAuth")
            };
            msg.To.Add(email);

            await client.SendMailAsync(msg);
            _logger.LogInformation("OTP email sent to {Email} for {Purpose}", email, purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }
}
