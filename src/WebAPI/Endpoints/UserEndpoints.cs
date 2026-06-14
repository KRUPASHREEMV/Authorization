using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoAuth.WebAPI.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        // GET /api/users/profile — requires User role
        app.MapGet("/api/users/profile", [Authorize(Roles = "User,Admin")] (HttpContext ctx) =>
        {
            var email = ctx.User.FindFirstValue(ClaimTypes.Email);
            var name = ctx.User.FindFirstValue(ClaimTypes.Name);
            var roles = ctx.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return Results.Ok(new { email, name, roles });
        })
        .WithName("GetProfile")
        .WithOpenApi()
        .RequireAuthorization();

        // GET /api/admin/users — requires Admin role
        app.MapGet("/api/admin/users", [Authorize(Roles = "Admin")] async (
            TodoAuth.Application.Services.IUserRepository userRepo) =>
        {
            var users = await userRepo.GetAllAsync();
            return Results.Ok(users.Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsVerified,
                u.CreatedAt
            }));
        })
        .WithName("GetAllUsers")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
