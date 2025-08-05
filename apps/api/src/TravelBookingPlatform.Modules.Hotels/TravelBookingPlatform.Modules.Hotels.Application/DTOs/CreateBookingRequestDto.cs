namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class CreateBookingRequestDto
{
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public int NumberOfRooms { get; set; } = 1;
}