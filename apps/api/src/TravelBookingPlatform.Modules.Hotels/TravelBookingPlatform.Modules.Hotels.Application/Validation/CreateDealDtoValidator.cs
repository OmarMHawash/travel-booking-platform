using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateDealDtoValidator : AbstractValidator<CreateDealDto>
{
    public CreateDealDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Deal title is required.")
            .MaximumLength(200).WithMessage("Deal title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Deal description is required.")
            .MaximumLength(2000).WithMessage("Deal description must not exceed 2000 characters.");

        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.RoomTypeId)
            .NotEmpty().WithMessage("Room type ID is required.");

        RuleFor(x => x.OriginalPrice)
            .GreaterThan(0).WithMessage("Original price must be greater than zero.")
            .LessThanOrEqualTo(100000).WithMessage("Original price must not exceed $100,000.");

        RuleFor(x => x.DiscountedPrice)
            .GreaterThan(0).WithMessage("Discounted price must be greater than zero.")
            .LessThanOrEqualTo(100000).WithMessage("Discounted price must not exceed $100,000.");

        RuleFor(x => x)
            .Must(x => x.DiscountedPrice < x.OriginalPrice)
            .WithMessage("Discounted price must be less than original price.")
            .WithName("DiscountedPrice");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("Valid from date is required.")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Valid from date cannot be in the past.");

        RuleFor(x => x.ValidTo)
            .NotEmpty().WithMessage("Valid to date is required.");

        RuleFor(x => x)
            .Must(x => x.ValidTo > x.ValidFrom)
            .WithMessage("Valid to date must be after valid from date.")
            .WithName("ValidTo");

        RuleFor(x => x.MaxBookings)
            .GreaterThan(0).WithMessage("Max bookings must be greater than zero.")
            .LessThanOrEqualTo(10000).WithMessage("Max bookings must not exceed 10,000.");

        RuleFor(x => x.ImageURL)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageURL));
    }
}