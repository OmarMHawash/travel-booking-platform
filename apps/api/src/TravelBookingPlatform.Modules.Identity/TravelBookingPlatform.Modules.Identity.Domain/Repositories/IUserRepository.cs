using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;

namespace TravelBookingPlatform.Modules.Identity.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}