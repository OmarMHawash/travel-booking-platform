namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class SearchRequestDto
{
    public string? SearchText { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; } = 1;
    public int Adults { get; set; } = 2;
    public int Children { get; set; } = 0;
    public decimal? MinRating { get; set; }
    public decimal? MaxRating { get; set; }
    public Guid? CityId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "Relevance";
}