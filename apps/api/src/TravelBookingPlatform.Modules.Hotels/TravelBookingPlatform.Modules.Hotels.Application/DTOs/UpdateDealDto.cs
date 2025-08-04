namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class UpdateDealDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid RoomTypeId { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsFeatured { get; set; }
    public int MaxBookings { get; set; }
    public string? ImageURL { get; set; }
}