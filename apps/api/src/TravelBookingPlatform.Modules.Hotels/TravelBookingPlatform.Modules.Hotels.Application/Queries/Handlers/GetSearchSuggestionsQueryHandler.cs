using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetSearchSuggestionsQueryHandler : IRequestHandler<GetSearchSuggestionsQuery, List<SearchSuggestionDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ICityRepository _cityRepository;

    public GetSearchSuggestionsQueryHandler(
        IHotelRepository hotelRepository,
        ICityRepository cityRepository)
    {
        _hotelRepository = hotelRepository;
        _cityRepository = cityRepository;
    }

    public async Task<List<SearchSuggestionDto>> Handle(GetSearchSuggestionsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchText) || request.SearchText.Length < 2)
            return new List<SearchSuggestionDto>();

        var suggestions = new List<SearchSuggestionDto>();

        // Get hotel suggestions
        var hotels = await _hotelRepository.GetHotelSuggestionsAsync(request.SearchText, request.MaxResults / 2);
        foreach (var hotel in hotels)
        {
            suggestions.Add(new SearchSuggestionDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Type = SearchSuggestionType.Hotel,
                Location = $"{hotel.City.Name}, {hotel.City.Country}",
                ImageUrl = hotel.ImageURL
            });
        }

        // Get city suggestions
        var cities = await _cityRepository.GetCitySuggestionsAsync(request.SearchText, request.MaxResults / 2);
        foreach (var city in cities)
        {
            suggestions.Add(new SearchSuggestionDto
            {
                Id = city.Id,
                Name = city.Name,
                Type = SearchSuggestionType.City,
                Location = $"{city.Name}, {city.Country}",
                ImageUrl = null
            });
        }

        return suggestions.Take(request.MaxResults).ToList();
    }
}