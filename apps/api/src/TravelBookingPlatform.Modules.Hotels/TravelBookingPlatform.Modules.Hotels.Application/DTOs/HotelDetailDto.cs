namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class HotelDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public List<HotelImageDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // City information
    public CityDto City { get; set; } = null!;

    // Room information
    public List<RoomDetailDto> Rooms { get; set; } = new();

    // Summary statistics
    public int TotalRooms { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string> AvailableRoomTypes { get; set; } = new();
}

public class RoomDetailDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Room type information
    public RoomTypeDetailDto RoomType { get; set; } = null!;
}

public class RoomTypeDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}