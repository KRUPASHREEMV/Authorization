using TodoAuth.Domain.Entities;

namespace TodoAuth.Application.Services;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(string id);
    Task<IEnumerable<ApplicationUser>> GetAllAsync();
    Task<ApplicationUser> CreateAsync(ApplicationUser user, string password);
    Task UpdateAsync(ApplicationUser user);
    Task DeleteAsync(ApplicationUser user);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task AddToRoleAsync(ApplicationUser user, string role);
}
