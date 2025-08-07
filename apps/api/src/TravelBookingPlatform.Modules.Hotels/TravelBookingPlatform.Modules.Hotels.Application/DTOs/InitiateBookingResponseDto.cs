namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class InitiateBookingResponseDto
{
    public Guid BookingId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
}