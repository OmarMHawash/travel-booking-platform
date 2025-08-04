using TravelBookingPlatform.Core.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;

namespace TravelBookingPlatform.Modules.Identity.Application.Commands;

public class RegisterUserCommand : ICommand<UserDto>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}