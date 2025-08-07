using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class GetHotelRoomAvailabilityQueryValidator : AbstractValidator<GetHotelRoomAvailabilityQuery>
{
    public GetHotelRoomAvailabilityQueryValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.CheckInDate)
            .NotEmpty().WithMessage("Check-in date is required.")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .NotEmpty().WithMessage("Check-out date is required.")
            .GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after the check-in date.");

        RuleFor(x => x.NumberOfAdults)
            .GreaterThan(0).WithMessage("Number of adults must be at least 1.");

        RuleFor(x => x.NumberOfChildren)
            .GreaterThanOrEqualTo(0).WithMessage("Number of children cannot be negative.");
    }
}