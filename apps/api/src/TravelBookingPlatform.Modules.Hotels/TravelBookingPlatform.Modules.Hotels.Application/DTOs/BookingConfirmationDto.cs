namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class BookingConfirmationDto
{
    public Guid BookingId { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int TotalNights { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Confirmed";
}