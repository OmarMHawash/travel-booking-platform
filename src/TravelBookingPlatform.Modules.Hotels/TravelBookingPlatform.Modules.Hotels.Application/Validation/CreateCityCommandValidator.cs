using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
{
    private readonly ICityRepository _cityRepository;

    public CreateCityCommandValidator(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("City Name is required")
            .MaximumLength(100).WithMessage("City Name must not exceed 100 characters")
            .MustAsync(async (name, cancellation) => !await _cityRepository.NameExistsAsync(name))
            .WithMessage("A city with this name already exists");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country Name is required")
            .MaximumLength(100).WithMessage("Country Name must not exceed 100 characters");

        RuleFor(x => x.PostCode)
            .NotEmpty().WithMessage("PostCode is required")
            .MaximumLength(20).WithMessage("PostCode must not exceed 20 characters")
            .MustAsync(async (postCode, cancellation) => !await _cityRepository.PostCodeExistsAsync(postCode))
            .WithMessage("A city with this post code already exists");
    }
}