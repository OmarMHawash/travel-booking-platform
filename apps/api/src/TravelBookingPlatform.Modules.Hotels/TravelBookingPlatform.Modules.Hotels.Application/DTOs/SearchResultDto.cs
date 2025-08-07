namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class SearchResultDto
{
    public List<HotelSearchResultDto> Hotels { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public TimeSpan QueryTime { get; set; }
}

public class HotelSearchResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal? PricePerNight { get; set; }
    public List<HotelImageDto> Images { get; set; } = new();
    public int AvailableRooms { get; set; }
    public bool IsAvailable { get; set; }
}