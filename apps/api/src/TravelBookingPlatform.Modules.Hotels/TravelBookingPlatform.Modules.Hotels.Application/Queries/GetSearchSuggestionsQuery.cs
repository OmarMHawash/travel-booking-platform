using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetSearchSuggestionsQuery(
    string SearchText,
    int MaxResults = 10) : IRequest<List<SearchSuggestionDto>>;