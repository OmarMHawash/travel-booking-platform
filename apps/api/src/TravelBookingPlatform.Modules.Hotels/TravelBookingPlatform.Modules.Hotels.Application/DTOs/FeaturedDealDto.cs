namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class FeaturedDealDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime ValidTo { get; set; }
    public string? ImageURL { get; set; }
    public HotelSummaryDto Hotel { get; set; } = new();
    public RoomTypeSummaryDto? RoomType { get; set; }
}

public class HotelSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ImageURL { get; set; }
}

public class RoomTypeSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }
}