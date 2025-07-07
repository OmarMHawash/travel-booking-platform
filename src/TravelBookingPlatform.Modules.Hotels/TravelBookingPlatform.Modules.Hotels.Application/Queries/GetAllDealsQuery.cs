using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Core.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetAllDealsQuery(int Page = 1, int PageSize = 20) : IQuery<List<DealDto>>;