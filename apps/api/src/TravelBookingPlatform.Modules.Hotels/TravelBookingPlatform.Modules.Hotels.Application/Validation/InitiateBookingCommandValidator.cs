using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class InitiateBookingCommandValidator : AbstractValidator<InitiateBookingCommand>
{
    public InitiateBookingCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.HotelId).NotEmpty();
        RuleFor(x => x.RoomTypeId).NotEmpty();

        RuleFor(x => x.CheckInDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .NotEmpty()
            .GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after check-in.");

        RuleFor(x => x.NumberOfAdults)
            .InclusiveBetween(1, 10).WithMessage("Number of adults must be between 1 and 10.");

        RuleFor(x => x.NumberOfChildren)
            .InclusiveBetween(0, 10).WithMessage("Number of children must be between 0 and 10.");

        RuleFor(x => x.GuestName)
            .NotEmpty().WithMessage("Guest name is required.")
            .MaximumLength(200).WithMessage("Guest name must not exceed 200 characters.");

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(1000).WithMessage("Special requests must not exceed 1000 characters.");
    }
}