using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
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

        RuleFor(x => x.NumberOfRooms)
            .InclusiveBetween(1, 5).WithMessage("You can book between 1 and 5 rooms at a time.");
    }
}