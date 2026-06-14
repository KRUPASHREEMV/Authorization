using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoAuth.Domain.Data;
using TodoAuth.Domain.Entities;
using TodoAuth.Infrastructure;

namespace TodoAuth.WebAPI.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("TodoAuth.Infrastructure")));

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // JWT Secret — env var with dev fallback
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        if (string.IsNullOrEmpty(jwtSecret))
        {
            if (env.IsDevelopment())
            {
                jwtSecret = config["Jwt:SecretKey"]
                    ?? throw new InvalidOperationException("Jwt:SecretKey is required in appsettings.json for development.");
                Console.WriteLine("⚠️  WARNING: Using JWT secret from appsettings.json. Set JWT_SECRET_KEY env var in production!");
            }
            else
            {
                throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required in production.");
            }
        }

        // JWT Authentication
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        // CORS — specific origin required for withCredentials (HttpOnly cookie)
        services.AddCors(o => o.AddPolicy("AllowAngular", p =>
            p.WithOrigins("http://localhost:4200")
             .AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials()));

        // Rate Limiting — built-in in .NET 7+
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddFixedWindowLimiter("OTPRateLimit", opt =>
            {
                opt.PermitLimit = 3;
                opt.Window = TimeSpan.FromHours(1);
                opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });
        });

        // Infrastructure services
        services.AddInfrastructureServices();

        // Command handlers (manual registration — no MediatR needed for this scale)
        services.AddScoped<TodoAuth.Application.Commands.RegisterUserCommandHandler>();
        services.AddScoped<TodoAuth.Application.Commands.AuthenticateUserCommandHandler>();
        services.AddScoped<TodoAuth.Application.Commands.VerifyOTPCommandHandler>();
        services.AddScoped<TodoAuth.Application.Commands.ResendOTPCommandHandler>();
        services.AddScoped<TodoAuth.Application.Commands.RequestPasswordResetCommandHandler>();
        services.AddScoped<TodoAuth.Application.Commands.VerifyResetOtpCommandHandler>();
        services.AddScoped<TodoAuth.Application.Commands.ResetPasswordCommandHandler>();
        services.AddScoped<TodoAuth.Application.Commands.RefreshTokenCommandHandler>();

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        return services;
    }
}
