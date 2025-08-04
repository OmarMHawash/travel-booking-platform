using AutoFixture;
using FluentAssertions;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.Validation;

namespace TravelBookingPlatform.Identity.UnitTests.Users.Commands;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;
    private readonly Fixture _fixture = new();

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public async Task Validator_ShouldHaveNoError_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "validuser")
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, "ValidPass123!")
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
    public async Task Validator_ShouldHaveError_WhenUsernameIsInvalid(string? invalidUsername)
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, invalidUsername!)
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, "ValidPass123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenUsernameIsTooShort()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "ab") // Too short
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, "ValidPass123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenUsernameIsTooLong()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, new string('a', 51)) // Too long
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, "ValidPass123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_ShouldHaveError_WhenEmailIsInvalid(string? invalidEmail)
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "validuser")
            .With(x => x.Email, invalidEmail!)
            .With(x => x.Password, "ValidPass123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("invalid@")]
    [InlineData("invalid.com")]
    public async Task Validator_ShouldHaveError_WhenEmailFormatIsInvalid(string invalidEmail)
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "validuser")
            .With(x => x.Email, invalidEmail)
            .With(x => x.Password, "ValidPass123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenEmailIsTooLong()
    {
        // Arrange
        var longEmail = new string('a', 90) + "@example.com"; // Too long
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "validuser")
            .With(x => x.Email, longEmail)
            .With(x => x.Password, "ValidPass123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_ShouldHaveError_WhenPasswordIsInvalid(string? invalidPassword)
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "validuser")
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, invalidPassword!)
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenPasswordIsTooShort()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "validuser")
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, "Short1!") // Too short
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("alllowercase1!")] // No uppercase
    [InlineData("ALLUPPERCASE1!")] // No lowercase
    [InlineData("NoNumbers!")] // No numbers
    [InlineData("NoSpecialChars123")] // No special characters
    [InlineData("SimplePassword")] // No numbers or special chars
    public async Task Validator_ShouldHaveError_WhenPasswordDoesNotMeetComplexityRequirements(string invalidPassword)
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "validuser")
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, invalidPassword)
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("ValidPass123!")]
    [InlineData("AnotherValid1@")]
    [InlineData("Complex$Pass9")]
    public async Task Validator_ShouldHaveNoError_WhenPasswordMeetsAllRequirements(string validPassword)
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "validuser")
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, validPassword)
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}