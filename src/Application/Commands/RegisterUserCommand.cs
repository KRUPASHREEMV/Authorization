using TodoAuth.Application.DTOs;
using TodoAuth.Application.Exceptions;
using TodoAuth.Application.Services;
using TodoAuth.Domain.Entities;

namespace TodoAuth.Application.Commands;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName);

public class RegisterUserCommandHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IOTPService _otpService;

    public RegisterUserCommandHandler(IUserRepository userRepo, IOTPService otpService)
    {
        _userRepo = userRepo;
        _otpService = otpService;
    }

    public async Task<RegistrationResult> Handle(RegisterUserCommand cmd)
    {
        var existing = await _userRepo.FindByEmailAsync(cmd.Email);
        if (existing != null)
            throw new AuthException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = cmd.Email,
            Email = cmd.Email,
            FirstName = cmd.FirstName,
            LastName = cmd.LastName,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(user, cmd.Password);
        await _userRepo.AddToRoleAsync(user, "User");
        await _otpService.GenerateOTPAsync(cmd.Email, "Registration");

        return new RegistrationResult("Registration successful! Please verify your email with the OTP sent.", true);
    }
}
