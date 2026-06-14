using Microsoft.AspNetCore.Identity;
using TodoAuth.Application.Commands;
using TodoAuth.Application.Exceptions;
using TodoAuth.Domain.Entities;
using TodoAuth.WebAPI.Helpers;

namespace TodoAuth.WebAPI.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        // POST /api/auth/register
        group.MapPost("/register", async (
            RegisterRequest req,
            RegisterUserCommandHandler handler) =>
        {
            try
            {
                var result = await handler.Handle(new RegisterUserCommand(req.Email, req.Password, req.FirstName, req.LastName));
                return Results.Ok(result);
            }
            catch (AuthException ex) { return Results.BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("Register")
        .WithOpenApi();

        // POST /api/auth/login
        group.MapPost("/login", async (
            LoginRequest req,
            AuthenticateUserCommandHandler handler,
            UserManager<ApplicationUser> userManager,
            HttpContext ctx,
            IWebHostEnvironment env) =>
        {
            try
            {
                var user = await userManager.FindByEmailAsync(req.Email);
                if (user == null)
                    return Results.BadRequest(new { message = "Invalid email or password." });

                var pwCheck = await userManager.CheckPasswordAsync(user, req.Password);
                if (!pwCheck)
                    return Results.BadRequest(new { message = "Invalid email or password." });

                if (!user.IsVerified)
                    return Results.BadRequest(new { message = "Please verify your email before logging in.", requiresVerification = true });

                var (result, plainRefreshToken) = await handler.Handle(new AuthenticateUserCommand(req.Email, req.Password));
                CookieHelper.SetRefreshTokenCookie(ctx.Response, plainRefreshToken, env.IsProduction());
                return Results.Ok(result);
            }
            catch (AuthException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("Login")
        .WithOpenApi();

        // POST /api/auth/verify-otp
        group.MapPost("/verify-otp", async (
            VerifyOtpRequest req,
            VerifyOTPCommandHandler handler,
            HttpContext ctx,
            IWebHostEnvironment env) =>
        {
            try
            {
                var (result, plainRefreshToken) = await handler.Handle(new VerifyOTPCommand(req.Email, req.Otp));
                CookieHelper.SetRefreshTokenCookie(ctx.Response, plainRefreshToken, env.IsProduction());
                return Results.Ok(result);
            }
            catch (AuthException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("VerifyOTP")
        .WithOpenApi();

        // POST /api/auth/resend-otp
        group.MapPost("/resend-otp", async (
            ResendOtpRequest req,
            ResendOTPCommandHandler handler) =>
        {
            try
            {
                var result = await handler.Handle(new ResendOTPCommand(req.Email, req.Purpose));
                return Results.Ok(result);
            }
            catch (AuthException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("ResendOTP")
        .WithOpenApi();

        // POST /api/auth/request-password-reset  (Step ①)
        group.MapPost("/request-password-reset", async (
            EmailOnlyRequest req,
            RequestPasswordResetCommandHandler handler) =>
        {
            try
            {
                var result = await handler.Handle(new RequestPasswordResetCommand(req.Email));
                return Results.Ok(result);
            }
            catch (AuthException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("RequestPasswordReset")
        .WithOpenApi();

        // POST /api/auth/verify-reset-otp  (Step ②)
        group.MapPost("/verify-reset-otp", async (
            VerifyOtpRequest req,
            VerifyResetOtpCommandHandler handler) =>
        {
            try
            {
                var result = await handler.Handle(new VerifyResetOtpCommand(req.Email, req.Otp));
                return Results.Ok(result);
            }
            catch (AuthException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("VerifyResetOtp")
        .WithOpenApi();

        // POST /api/auth/reset-password  (Step ③)
        group.MapPost("/reset-password", async (
            ResetPasswordRequest req,
            ResetPasswordCommandHandler handler) =>
        {
            try
            {
                var result = await handler.Handle(new ResetPasswordCommand(req.Email, req.ResetToken, req.NewPassword));
                return Results.Ok(result);
            }
            catch (AuthException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("ResetPassword")
        .WithOpenApi();

        // POST /api/auth/refresh  — browser auto-sends HttpOnly cookie
        group.MapPost("/refresh", async (
            HttpContext ctx,
            RefreshTokenCommandHandler handler,
            IWebHostEnvironment env) =>
        {
            var plainRefreshToken = ctx.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(plainRefreshToken))
                return Results.Unauthorized();

            try
            {
                var (result, newPlainRefreshToken) = await handler.Handle(new RefreshTokenCommand(plainRefreshToken));
                CookieHelper.SetRefreshTokenCookie(ctx.Response, newPlainRefreshToken, env.IsProduction());
                return Results.Ok(result);
            }
            catch (AuthException) { return Results.Unauthorized(); }
        })
        .WithName("RefreshToken")
        .WithOpenApi();

        // POST /api/auth/logout
        group.MapPost("/logout", async (
            LogoutRequest req,
            TodoAuth.Application.Services.IUserRepository userRepo,
            HttpContext ctx) =>
        {
            if (!string.IsNullOrEmpty(req.Email))
            {
                var user = await userRepo.FindByEmailAsync(req.Email);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiresAt = null;
                    await userRepo.UpdateAsync(user);
                }
            }
            CookieHelper.ClearRefreshTokenCookie(ctx.Response);
            return Results.Ok(new { message = "Logged out successfully." });
        })
        .WithName("Logout")
        .WithOpenApi();
    }
}

// Request record types
record RegisterRequest(string Email, string Password, string FirstName, string LastName);
record LoginRequest(string Email, string Password);
record VerifyOtpRequest(string Email, string Otp);
record ResendOtpRequest(string Email, string Purpose);
record EmailOnlyRequest(string Email);
record ResetPasswordRequest(string Email, string ResetToken, string NewPassword);
record LogoutRequest(string? Email);
