using UserManagement.Domain.Entities;

namespace UserManagement.Application.Contracts;

public interface IAuthRepository
{
   Task AddUserAsync(ApplicationUser user);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
   Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
    Task<bool> UserExistsAsync(string email);
   Task UpdateUserAsync(ApplicationUser user);
    Task DeleteUserAsync(Guid userId);
}