using TodoAuth.Application.DTOs;
using TodoAuth.Application.Exceptions;
using TodoAuth.Application.Services;

namespace TodoAuth.Application.Commands;

public record ResendOTPCommand(string Email, string Purpose);

public class ResendOTPCommandHandler
{
    private readonly IOTPService _otpService;

    public ResendOTPCommandHandler(IOTPService otpService) => _otpService = otpService;

    public async Task<ResendOTPResult> Handle(ResendOTPCommand cmd)
    {
        var isRateLimited = await _otpService.IsRateLimitedAsync(cmd.Email);
        if (isRateLimited)
            throw new AuthException("Too many OTP requests. Please wait 1 hour.");

        await _otpService.GenerateOTPAsync(cmd.Email, cmd.Purpose);
        return new ResendOTPResult("New OTP sent to your email!");
    }
}
