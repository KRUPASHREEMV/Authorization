using Microsoft.AspNetCore.Identity;
using TodoAuth.Domain.Entities;
using TodoAuth.WebAPI.Endpoints;
using TodoAuth.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// SECURITY: Enforce HTTPS in production only (dev uses HTTP with dev cert)
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// Seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapAuthEndpoints();
app.MapUserEndpoints();

app.Run();

