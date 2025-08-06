namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorDisplayName { get; set; } = "Anonymous"; // Default value; can be populated via inter-module communication
    public int StarRating { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}