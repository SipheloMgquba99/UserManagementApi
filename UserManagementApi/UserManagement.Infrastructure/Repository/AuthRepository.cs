using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UserManagement.Application.Contracts;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMemoryCache _cache;

        public AuthRepository(AppDbContext appDbContext, IMemoryCache cache)
        {
            _appDbContext = appDbContext;
            _cache = cache;
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            var cacheKey = $"user_email_{email}";
            if (_cache.TryGetValue(cacheKey, out ApplicationUser? user))
                return user;

            user = await _appDbContext.ApplicationUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
            }

            return user;
        }

        public async Task AddUserAsync(ApplicationUser user)
        {
            _appDbContext.ApplicationUsers.Add(user);
            await _appDbContext.SaveChangesAsync();

            var cacheKey = $"user_email_{user.Email}";
            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
        {
            return await _appDbContext.ApplicationUsers.FindAsync(userId);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _appDbContext.ApplicationUsers.AnyAsync(u => u.Email == email);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _appDbContext.ApplicationUsers.Update(user);
            await _appDbContext.SaveChangesAsync();

            var cacheKey = $"user_email_{user.Email}";
            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _appDbContext.ApplicationUsers.FindAsync(userId);
            if (user == null)
                return;

            _appDbContext.ApplicationUsers.Remove(user);
            await _appDbContext.SaveChangesAsync();

            var cacheKey = $"user_email_{user.Email}";
            _cache.Remove(cacheKey);
        }
    }
}
