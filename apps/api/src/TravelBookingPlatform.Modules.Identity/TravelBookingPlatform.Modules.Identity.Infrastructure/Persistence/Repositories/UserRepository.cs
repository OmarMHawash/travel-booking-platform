using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Identity.Infrastructure.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant());
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email.Value == email.ToLowerInvariant());
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }
}