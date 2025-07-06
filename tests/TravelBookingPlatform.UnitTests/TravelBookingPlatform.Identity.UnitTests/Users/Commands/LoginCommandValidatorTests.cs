using AutoFixture;
using FluentAssertions;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.Validation;

namespace TravelBookingPlatform.Identity.UnitTests.Users.Commands;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;
    private readonly Fixture _fixture = new();

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public async Task Validator_ShouldHaveNoError_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "valid@example.com")
            .With(x => x.Password, "ValidPassword123!")
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
    public async Task Validator_ShouldHaveError_WhenEmailIsInvalid(string? invalidEmail)
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, invalidEmail!)
            .With(x => x.Password, "ValidPassword123!")
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
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, invalidEmail)
            .With(x => x.Password, "ValidPassword123!")
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
        var command = _fixture.Build<LoginCommand>()
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
    public async Task Validator_ShouldHaveNoError_WhenEmailAndPasswordAreValid()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "user@example.com")
            .With(x => x.Password, "anypassword") // Login validation doesn't check password complexity
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validator_ShouldHaveNoError_WhenUsingRandomValidData()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, _fixture.Create<string>() + "@example.com")
            .With(x => x.Password, _fixture.Create<string>())
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}