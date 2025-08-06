namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class CreateReviewRequestDto
{
    public int StarRating { get; set; }
    public string Description { get; set; } = string.Empty;
}