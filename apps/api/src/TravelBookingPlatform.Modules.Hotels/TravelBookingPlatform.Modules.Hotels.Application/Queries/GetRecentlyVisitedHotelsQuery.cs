using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetRecentlyVisitedHotelsQuery(Guid UserId, int Limit = 10) : IRequest<List<RecentlyVisitedHotelDto>>;