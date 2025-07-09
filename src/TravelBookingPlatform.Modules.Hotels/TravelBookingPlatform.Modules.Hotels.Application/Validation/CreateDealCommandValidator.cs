using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateDealCommandValidator : AbstractValidator<CreateDealCommand>
{
    public CreateDealCommandValidator()
    {
        // Only validate that DealData is not null - field validation is handled by CreateDealDtoValidator
        RuleFor(x => x.DealData)
            .NotNull().WithMessage("Deal data is required.");
    }
}