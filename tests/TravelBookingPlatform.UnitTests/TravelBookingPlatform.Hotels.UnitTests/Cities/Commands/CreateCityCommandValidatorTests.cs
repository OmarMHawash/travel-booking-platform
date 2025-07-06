using AutoFixture;
using FluentAssertions;
using NSubstitute;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.Validation;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Hotels.UnitTests.Cities.Commands;

public class CreateCityCommandValidatorTests
{
    private readonly ICityRepository _mockCityRepository;
    private readonly CreateCityCommandValidator _validator;
    private readonly Fixture _fixture = new();

    public CreateCityCommandValidatorTests()
    {
        _mockCityRepository = Substitute.For<ICityRepository>();

        // Setup default behavior - no existing cities with same name or postcode
        _mockCityRepository.NameExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>())
            .Returns(false);
        _mockCityRepository.PostCodeExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>())
            .Returns(false);

        _validator = new CreateCityCommandValidator(_mockCityRepository);
    }

    [Fact]
    public async Task Validator_ShouldHaveNoError_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "New York")
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validator_ShouldHaveNoError_WhenCommandIsValidWithRandomData()
    {
        // Arrange - Using AutoFixture to generate valid random data
        var randomName = _fixture.Create<string>();
        var randomCountry = _fixture.Create<string>();
        var randomPostCode = _fixture.Create<string>();

        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, randomName.Length > 100 ? randomName[..50] : randomName) // Ensure name is within limits
            .With(x => x.Country, randomCountry.Length > 100 ? randomCountry[..50] : randomCountry) // Ensure country is within limits
            .With(x => x.PostCode, randomPostCode.Length > 20 ? randomPostCode[..10] : randomPostCode) // Ensure postcode is within limits
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_ShouldHaveError_WhenNameIsInvalid(string? name)
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, name!)
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_ShouldHaveError_WhenCountryIsInvalid(string? country)
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "New York")
            .With(x => x.Country, country!)
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Country");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenNameIsTooLong()
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, new string('a', 101)) // 101 characters (exceeds the 100 character limit)
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenCountryIsTooLong()
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "New York")
            .With(x => x.Country, new string('a', 101)) // 101 characters (assuming country has similar limit)
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Country");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenNameAlreadyExists()
    {
        // Arrange
        _mockCityRepository.NameExistsAsync("Existing City", Arg.Any<Guid?>())
            .Returns(true);

        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "Existing City")
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("already exists"));
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenPostCodeAlreadyExists()
    {
        // Arrange
        _mockCityRepository.PostCodeExistsAsync("12345", Arg.Any<Guid?>())
            .Returns(true);

        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "New City")
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, "12345")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PostCode" && e.ErrorMessage.Contains("already exists"));
    }

    [Fact]
    public async Task Validator_ShouldHaveNoError_WhenNameAndPostCodeAreUnique()
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "Unique City")
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, "54321")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}