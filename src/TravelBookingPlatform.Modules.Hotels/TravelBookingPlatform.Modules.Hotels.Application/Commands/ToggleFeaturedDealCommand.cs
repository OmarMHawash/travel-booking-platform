using TravelBookingPlatform.Core.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public record ToggleFeaturedDealCommand(Guid Id) : ICommand;