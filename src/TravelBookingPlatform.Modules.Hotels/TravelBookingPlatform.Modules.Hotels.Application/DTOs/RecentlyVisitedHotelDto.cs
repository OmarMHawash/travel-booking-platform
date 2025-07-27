namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class RecentlyVisitedHotelDto
{
    public Guid HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime LastVisitedDate { get; set; }
    public int VisitCount { get; set; }

    // City information
    public Guid CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Price range information (from room types)
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}