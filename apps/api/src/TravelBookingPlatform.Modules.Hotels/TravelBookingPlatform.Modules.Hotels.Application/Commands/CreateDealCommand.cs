using TravelBookingPlatform.Core.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public record CreateDealCommand(CreateDealDto DealData) : ICommand<Guid>;