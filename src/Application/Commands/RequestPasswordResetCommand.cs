using TodoAuth.Application.DTOs;
using TodoAuth.Application.Exceptions;
using TodoAuth.Application.Services;

namespace TodoAuth.Application.Commands;

public record RequestPasswordResetCommand(string Email);

public class RequestPasswordResetCommandHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IOTPService _otpService;

    public RequestPasswordResetCommandHandler(IUserRepository userRepo, IOTPService otpService)
    {
        _userRepo = userRepo;
        _otpService = otpService;
    }

    public async Task<PasswordResetResult> Handle(RequestPasswordResetCommand cmd)
    {
        var user = await _userRepo.FindByEmailAsync(cmd.Email);
        // Generic response — do not reveal whether email exists (prevents enumeration)
        if (user == null)
            return new PasswordResetResult("If an account with this email exists, an OTP has been sent.", true);

        var isRateLimited = await _otpService.IsRateLimitedAsync(cmd.Email);
        if (isRateLimited)
            throw new AuthException("Too many OTP requests. Please wait 1 hour.");

        await _otpService.GenerateOTPAsync(cmd.Email, "PasswordReset");
        return new PasswordResetResult("If an account with this email exists, an OTP has been sent.", true);
    }
}
