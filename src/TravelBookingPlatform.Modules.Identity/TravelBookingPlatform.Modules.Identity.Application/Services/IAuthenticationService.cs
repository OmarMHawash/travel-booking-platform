using TravelBookingPlatform.Modules.Identity.Domain.Entities;

namespace TravelBookingPlatform.Modules.Identity.Application.Services;

public interface IAuthenticationService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    Task<bool> ValidateUserAsync(string email, string password);
    Task<User?> AuthenticateAsync(string email, string password);
}