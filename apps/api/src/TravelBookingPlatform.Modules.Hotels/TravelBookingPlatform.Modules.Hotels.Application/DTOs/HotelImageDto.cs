namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class HotelImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
    public bool IsCoverImage { get; set; }
}