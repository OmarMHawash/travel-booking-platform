namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class BookingDetailsDto
{
    public Guid BookingId { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }

    // Hotel Details
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Room & Stay Details
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int TotalNights { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookedAt { get; set; }

    // User Details
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
}