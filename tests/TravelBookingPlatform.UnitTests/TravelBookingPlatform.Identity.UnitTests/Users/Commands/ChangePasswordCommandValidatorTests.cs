using AutoFixture;
using FluentAssertions;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.Validation;

namespace TravelBookingPlatform.Identity.UnitTests.Users.Commands;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;
    private readonly Fixture _fixture = new();

    public ChangePasswordCommandValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }

    [Fact]
    public async Task Validator_ShouldHaveNoError_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.NewGuid())
            .With(x => x.CurrentPassword, "CurrentPassword123!")
            .With(x => x.NewPassword, "NewPassword123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.Empty)
            .With(x => x.CurrentPassword, "CurrentPassword123!")
            .With(x => x.NewPassword, "NewPassword123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_ShouldHaveError_WhenCurrentPasswordIsInvalid(string? invalidCurrentPassword)
    {
        // Arrange
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.NewGuid())
            .With(x => x.CurrentPassword, invalidCurrentPassword!)
            .With(x => x.NewPassword, "NewPassword123!")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CurrentPassword");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_ShouldHaveError_WhenNewPasswordIsInvalid(string? invalidNewPassword)
    {
        // Arrange
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.NewGuid())
            .With(x => x.CurrentPassword, "CurrentPassword123!")
            .With(x => x.NewPassword, invalidNewPassword!)
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenNewPasswordIsTooShort()
    {
        // Arrange
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.NewGuid())
            .With(x => x.CurrentPassword, "CurrentPassword123!")
            .With(x => x.NewPassword, "Short1!") // Too short
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Theory]
    [InlineData("alllowercase1!")] // No uppercase
    [InlineData("ALLUPPERCASE1!")] // No lowercase
    [InlineData("NoNumbers!")] // No numbers
    [InlineData("NoSpecialChars123")] // No special characters
    [InlineData("SimplePassword")] // No numbers or special chars
    public async Task Validator_ShouldHaveError_WhenNewPasswordDoesNotMeetComplexityRequirements(string invalidPassword)
    {
        // Arrange
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.NewGuid())
            .With(x => x.CurrentPassword, "CurrentPassword123!")
            .With(x => x.NewPassword, invalidPassword)
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenNewPasswordEqualsCurrentPassword()
    {
        // Arrange
        var samePassword = "SamePassword123!";
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.NewGuid())
            .With(x => x.CurrentPassword, samePassword)
            .With(x => x.NewPassword, samePassword)
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("different from current password"));
    }

    [Theory]
    [InlineData("ValidNewPass123!")]
    [InlineData("AnotherValid1@")]
    [InlineData("Complex$Pass9")]
    public async Task Validator_ShouldHaveNoError_WhenNewPasswordMeetsAllRequirements(string validNewPassword)
    {
        // Arrange
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.NewGuid())
            .With(x => x.CurrentPassword, "DifferentCurrentPassword123!")
            .With(x => x.NewPassword, validNewPassword)
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
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, Guid.NewGuid())
            .With(x => x.CurrentPassword, "CurrentPassword123!")
            .With(x => x.NewPassword, "NewPassword456@")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}