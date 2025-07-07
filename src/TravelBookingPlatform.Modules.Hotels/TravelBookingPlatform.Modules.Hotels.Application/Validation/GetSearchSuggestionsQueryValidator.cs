using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class GetSearchSuggestionsQueryValidator : AbstractValidator<GetSearchSuggestionsQuery>
{
    public GetSearchSuggestionsQueryValidator()
    {
        RuleFor(x => x.SearchText)
            .NotEmpty().WithMessage("Search text is required and cannot be empty.")
            .MinimumLength(2).WithMessage("Search text must be at least 2 characters long.");

        RuleFor(x => x.MaxResults)
            .InclusiveBetween(1, 50).WithMessage("Limit must be between 1 and 50.");
    }
}