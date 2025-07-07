using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class UpdateDealCommandValidator : AbstractValidator<UpdateDealCommand>
{
    public UpdateDealCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Deal ID is required.");

        // First, validate that DealData is not null
        RuleFor(x => x.DealData)
            .NotNull().WithMessage("Deal data is required.");

        // Apply other validation rules only when DealData is not null
        When(x => x.DealData != null, () =>
        {
            RuleFor(x => x.DealData.Title)
                .NotEmpty().WithMessage("Deal title is required.")
                .MaximumLength(200).WithMessage("Deal title must not exceed 200 characters.");

            RuleFor(x => x.DealData.Description)
                .NotEmpty().WithMessage("Deal description is required.")
                .MaximumLength(2000).WithMessage("Deal description must not exceed 2000 characters.");

            RuleFor(x => x.DealData.RoomTypeId)
                .NotEmpty().WithMessage("Room type ID is required.");

            RuleFor(x => x.DealData.OriginalPrice)
                .GreaterThan(0).WithMessage("Original price must be greater than zero.")
                .LessThanOrEqualTo(100000).WithMessage("Original price must not exceed $100,000.");

            RuleFor(x => x.DealData.DiscountedPrice)
                .GreaterThan(0).WithMessage("Discounted price must be greater than zero.")
                .LessThanOrEqualTo(100000).WithMessage("Discounted price must not exceed $100,000.");

            RuleFor(x => x.DealData)
                .Must(x => x.DiscountedPrice < x.OriginalPrice)
                .WithMessage("Discounted price must be less than original price.")
                .WithName("DiscountedPrice");

            RuleFor(x => x.DealData.ValidFrom)
                .NotEmpty().WithMessage("Valid from date is required.");

            RuleFor(x => x.DealData.ValidTo)
                .NotEmpty().WithMessage("Valid to date is required.");

            RuleFor(x => x.DealData)
                .Must(x => x.ValidTo > x.ValidFrom)
                .WithMessage("Valid to date must be after valid from date.")
                .WithName("ValidTo");

            RuleFor(x => x.DealData.MaxBookings)
                .GreaterThan(0).WithMessage("Max bookings must be greater than zero.")
                .LessThanOrEqualTo(10000).WithMessage("Max bookings must not exceed 10,000.");

            RuleFor(x => x.DealData.ImageURL)
                .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.")
                .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Image URL must be a valid URL.")
                .When(x => !string.IsNullOrWhiteSpace(x.DealData.ImageURL));
        });
    }
}