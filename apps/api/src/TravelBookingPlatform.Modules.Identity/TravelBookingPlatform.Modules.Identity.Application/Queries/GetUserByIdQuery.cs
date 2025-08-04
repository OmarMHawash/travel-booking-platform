using TravelBookingPlatform.Core.Application.Queries;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;

namespace TravelBookingPlatform.Modules.Identity.Application.Queries;

public class GetUserByIdQuery : IQuery<UserDto?>
{
    public Guid UserId { get; set; }
}