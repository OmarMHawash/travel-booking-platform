using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record SearchHotelsQuery(SearchRequestDto SearchRequest) : IRequest<SearchResultDto>;