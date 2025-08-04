using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;

namespace TravelBookingPlatform.Modules.Identity.Application.Services;

public interface ITokenService
{
    TokenDto GenerateToken(User user);
    bool ValidateToken(string token);
}