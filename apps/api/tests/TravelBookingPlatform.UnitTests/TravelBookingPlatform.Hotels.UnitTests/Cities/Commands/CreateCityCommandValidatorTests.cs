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
            .With(x => x.Country, "United States")
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("London", "United Kingdom", "SW1A 1AA")]
    [InlineData("Paris", "France", "75001")]
    [InlineData("Toronto", "Canada", "M5V 3A8")]
    [InlineData("New York", "USA", "12345-6789")]
    [InlineData("Amsterdam", "Netherlands", "1012 NX")]
    public async Task Validator_ShouldHaveNoError_WhenCommandIsValidWithVariousFormats(string name, string country, string postCode)
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, name)
            .With(x => x.Country, country)
            .With(x => x.PostCode, postCode)
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
    [InlineData("A")] // Too short
    [InlineData("1")] // Number only
    [InlineData("New York123")] // Contains numbers
    [InlineData("City@Name")] // Invalid characters
    public async Task Validator_ShouldHaveError_WhenNameIsInvalidFormat(string name)
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, name)
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

    [Theory]
    [InlineData("A")] // Too short
    [InlineData("1")] // Number only
    [InlineData("USA123")] // Contains numbers
    [InlineData("Country@Name")] // Invalid characters
    public async Task Validator_ShouldHaveError_WhenCountryIsInvalidFormat(string country)
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "New York")
            .With(x => x.Country, country)
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Country");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12")] // Too short
    [InlineData("INVALID@CODE")] // Invalid characters

    public async Task Validator_ShouldHaveError_WhenPostCodeIsInvalid(string? postCode)
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "New York")
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, postCode!)
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PostCode");
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
            .With(x => x.Country, new string('a', 101)) // 101 characters
            .With(x => x.PostCode, "10001")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Country");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenPostCodeIsTooLong()
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "New York")
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, new string('1', 21)) // 21 characters
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PostCode");
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

    [Theory]
    [InlineData("Saint-Pierre")] // Hyphen
    [InlineData("O'Connor")] // Apostrophe
    [InlineData("St. Louis")] // Period
    public async Task Validator_ShouldHaveNoError_WhenNameContainsValidSpecialCharacters(string name)
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, name)
            .With(x => x.Country, "USA")
            .With(x => x.PostCode, "12345")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("United Kingdom")] // Space
    [InlineData("Saint-Martin")] // Hyphen
    [InlineData("Côte d'Ivoire")] // Apostrophe (if supported)
    public async Task Validator_ShouldHaveNoError_WhenCountryContainsValidSpecialCharacters(string country)
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "TestCity")
            .With(x => x.Country, country)
            .With(x => x.PostCode, "12345")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}