using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoAuth.Application.Services;
using TodoAuth.Domain.Data;
using TodoAuth.Domain.Entities;

namespace TodoAuth.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email) =>
        await _userManager.FindByEmailAsync(email);

    public async Task<ApplicationUser?> FindByIdAsync(string id) =>
        await _userManager.FindByIdAsync(id);

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync() =>
        await _dbContext.Users.ToListAsync();

    public async Task<ApplicationUser> CreateAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }
        return user;
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }
    }

    public async Task DeleteAsync(ApplicationUser user)
    {
        await _userManager.DeleteAsync(user);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user) =>
        await _userManager.GetRolesAsync(user);

    public async Task AddToRoleAsync(ApplicationUser user, string role) =>
        await _userManager.AddToRoleAsync(user, role);
}
