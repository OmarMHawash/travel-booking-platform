using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetPopularDestinationsQuery(int Count = 5) : IRequest<List<CityDto>>;