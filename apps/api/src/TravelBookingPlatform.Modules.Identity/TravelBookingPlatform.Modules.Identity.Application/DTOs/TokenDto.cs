namespace TravelBookingPlatform.Modules.Identity.Application.DTOs;

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}