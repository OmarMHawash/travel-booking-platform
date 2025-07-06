using TravelBookingPlatform.Core.Application.Queries;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;

namespace TravelBookingPlatform.Modules.Identity.Application.Queries;

public class GetUserByEmailQuery : IQuery<UserDto?>
{
    public string Email { get; set; } = string.Empty;
}