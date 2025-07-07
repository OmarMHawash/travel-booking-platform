using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using System.Linq;
using System.Text.RegularExpressions;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
{
    private readonly ICityRepository _cityRepository;

    public CreateCityCommandValidator(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("City Name is required")
            .MinimumLength(2).WithMessage("City Name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("City Name must not exceed 100 characters")
            .Matches(@"^[\p{L}\s\-'\.]+$").WithMessage("City Name can only contain letters, spaces, hyphens, apostrophes, and periods")
            .MustAsync(async (name, cancellation) => !await _cityRepository.NameExistsAsync(name))
            .WithMessage("A city with this name already exists");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country Name is required")
            .MinimumLength(2).WithMessage("Country Name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("Country Name must not exceed 100 characters")
            .Matches(@"^[\p{L}\s\-'\.]+$").WithMessage("Country Name can only contain letters, spaces, hyphens, apostrophes, and periods (no numbers allowed)");

        RuleFor(x => x.PostCode)
            .NotEmpty().WithMessage("PostCode is required")
            .MinimumLength(3).WithMessage("PostCode must be at least 3 characters long")
            .MaximumLength(20).WithMessage("PostCode must not exceed 20 characters")
            .Matches(@"^[A-Za-z0-9\s\-]+$").WithMessage("PostCode can only contain letters, numbers, spaces, and hyphens")
            .Must(BeValidPostCodeFormat).WithMessage("PostCode must follow a valid format (e.g., 12345, SW1A 1AA, K1A-0A6)")
            .MustAsync(async (postCode, cancellation) => !await _cityRepository.PostCodeExistsAsync(postCode))
            .WithMessage("A city with this post code already exists");
    }

    private bool BeValidPostCodeFormat(string postCode)
    {
        if (string.IsNullOrEmpty(postCode))
            return false;

        // Common postcode patterns:
        // US: 5 digits (12345) or 9 digits (12345-6789)
        // UK: Various formats like SW1A 1AA, M1 1AA, B33 8TH
        // Canada: A1A 1A1 or A1A-1A1
        // General: Allow alphanumeric with spaces and hyphens

        var patterns = new[]
        {
            @"^\d{5}$",                           // US 5-digit
            @"^\d{5}-\d{4}$",                    // US 9-digit
            @"^[A-Z]{1,2}\d{1,2}[A-Z]?\s?\d[A-Z]{2}$", // UK format
            @"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$",      // Canada
            @"^[A-Z]\d[A-Z]-\d[A-Z]\d$",        // Canada with hyphen
            @"^\d{4}\s?[A-Z]{2}$",              // Netherlands
            @"^\d{5}$",                         // Germany
            @"^\d{3}-\d{4}$",                   // Some other formats
            @"^[A-Za-z0-9]+$"                   // Custom pattern: letters and digits only, e.g. P405, 405A, C405A
        };
        
        return patterns.Any(pattern => Regex.IsMatch(postCode.ToUpper(), pattern));
    }
}