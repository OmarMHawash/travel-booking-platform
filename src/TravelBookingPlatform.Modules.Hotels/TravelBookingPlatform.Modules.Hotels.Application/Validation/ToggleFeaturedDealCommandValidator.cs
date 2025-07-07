using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class ToggleFeaturedDealCommandValidator : AbstractValidator<ToggleFeaturedDealCommand>
{
    public ToggleFeaturedDealCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Deal ID is required.");
    }
}