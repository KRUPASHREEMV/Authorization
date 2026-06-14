using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TodoAuth.Application.Services;
using TodoAuth.Domain.Entities;
using TodoAuth.Infrastructure.Repositories;
using TodoAuth.Infrastructure.Services;

namespace TodoAuth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOTPService, OTPService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

        return services;
    }
}
