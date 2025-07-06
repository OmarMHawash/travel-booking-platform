using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class SearchRequestDtoValidator : AbstractValidator<SearchRequestDto>
{
    public SearchRequestDtoValidator()
    {
        RuleFor(x => x.SearchText)
            .MinimumLength(2)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText))
            .WithMessage("Search text must be at least 2 characters long.")
            .MaximumLength(500)
            .WithMessage("Search text cannot exceed 500 characters.");

        RuleFor(x => x.CheckInDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .When(x => x.CheckInDate.HasValue)
            .WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .GreaterThan(x => x.CheckInDate)
            .When(x => x.CheckInDate.HasValue && x.CheckOutDate.HasValue)
            .WithMessage("Check-out date must be after check-in date.")
            .LessThanOrEqualTo(DateTime.Today.AddYears(2))
            .When(x => x.CheckOutDate.HasValue)
            .WithMessage("Check-out date cannot be more than 2 years in the future.");

        RuleFor(x => x.NumberOfRooms)
            .GreaterThan(0)
            .LessThanOrEqualTo(10)
            .WithMessage("Number of rooms must be between 1 and 10.");

        RuleFor(x => x.Adults)
            .GreaterThan(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Number of adults must be between 1 and 20.");

        RuleFor(x => x.Children)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Number of children must be between 0 and 20.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5)
            .When(x => x.MinRating.HasValue)
            .WithMessage("Minimum rating must be between 0 and 5.");

        RuleFor(x => x.MaxRating)
            .InclusiveBetween(0, 5)
            .When(x => x.MaxRating.HasValue)
            .WithMessage("Maximum rating must be between 0 and 5.");

        RuleFor(x => x.MaxRating)
            .GreaterThanOrEqualTo(x => x.MinRating)
            .When(x => x.MinRating.HasValue && x.MaxRating.HasValue)
            .WithMessage("Maximum rating must be greater than or equal to minimum rating.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "Relevance", "Price", "Rating", "Name" }.Contains(sortBy))
            .WithMessage("Sort by must be one of: Relevance, Price, Rating, Name.");
    }
}