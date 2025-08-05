using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetUserBookingsQuery(Guid UserId) : IRequest<List<UserBookingDto>>;