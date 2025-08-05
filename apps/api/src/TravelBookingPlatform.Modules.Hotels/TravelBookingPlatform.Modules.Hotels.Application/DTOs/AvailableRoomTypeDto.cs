namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class AvailableRoomTypeDto
{
    public Guid RoomTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }
    public string? ImageUrl { get; set; }
    public int NumberOfAvailableRooms { get; set; }
}