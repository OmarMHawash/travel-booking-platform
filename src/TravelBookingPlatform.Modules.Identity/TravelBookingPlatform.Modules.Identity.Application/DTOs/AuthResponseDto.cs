namespace TravelBookingPlatform.Modules.Identity.Application.DTOs;

public class AuthResponseDto
{
    public TokenDto Token { get; set; } = new();
    public UserDto User { get; set; } = new();
}