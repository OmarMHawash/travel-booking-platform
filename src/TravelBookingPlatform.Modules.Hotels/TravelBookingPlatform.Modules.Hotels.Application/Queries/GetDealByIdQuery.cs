using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Core.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetDealByIdQuery(Guid Id) : IQuery<DealDto?>;