using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Core.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetFeaturedDealsQuery(int Count = 10) : IQuery<List<FeaturedDealDto>>;