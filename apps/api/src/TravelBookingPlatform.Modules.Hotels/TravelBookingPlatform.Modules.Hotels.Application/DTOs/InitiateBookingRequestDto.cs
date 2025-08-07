namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class InitiateBookingRequestDto
{
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }
}