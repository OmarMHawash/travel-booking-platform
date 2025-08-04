namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class SearchSuggestionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SearchSuggestionType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public enum SearchSuggestionType
{
    Hotel,
    City
}