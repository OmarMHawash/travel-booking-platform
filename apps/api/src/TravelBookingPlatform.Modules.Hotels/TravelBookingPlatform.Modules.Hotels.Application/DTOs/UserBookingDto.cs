namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class UserBookingDto
{
    public Guid BookingId { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty; // e.g., "Upcoming", "In Progress", "Completed"

    // Hotel Details
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string? HotelImageUrl { get; set; }
    public string CityName { get; set; } = string.Empty;

    // Room & Stay Details
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int TotalNights { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookedAt { get; set; }
}