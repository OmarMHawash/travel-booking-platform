using AutoFixture;
using FluentAssertions;
using NSubstitute;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.Commands.Handlers;
using TravelBookingPlatform.Modules.Identity.Application.Services;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;

namespace TravelBookingPlatform.Identity.UnitTests.Users.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;
    private readonly ChangePasswordCommandHandler _handler;
    private readonly Fixture _fixture;

    public ChangePasswordCommandHandlerTests()
    {
        _fixture = new Fixture();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _authenticationService = Substitute.For<IAuthenticationService>();

        _handler = new ChangePasswordCommandHandler(_userRepository, _unitOfWork, _authenticationService);
    }

    [Fact]
    public async Task Handle_ShouldChangePasswordAndReturnTrue_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, userId)
            .With(x => x.CurrentPassword, "OldPassword123!")
            .With(x => x.NewPassword, "NewPassword123!")
            .Create();

        var user = new User("testuser", "test@example.com", "hashedOldPassword");
        var newHashedPassword = "hashedNewPassword";

        _userRepository.GetByIdAsync(userId).Returns(user);
        _authenticationService.VerifyPassword(command.CurrentPassword, user.PasswordHash).Returns(true);
        _authenticationService.HashPassword(command.NewPassword).Returns(newHashedPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.PasswordHash.Should().Be(newHashedPassword);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldChangePasswordWithRandomData_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, userId)
            .Create();

        var user = new User(_fixture.Create<string>(), _fixture.Create<string>() + "@example.com", _fixture.Create<string>());
        var newHashedPassword = _fixture.Create<string>();

        _userRepository.GetByIdAsync(userId).Returns(user);
        _authenticationService.VerifyPassword(command.CurrentPassword, user.PasswordHash).Returns(true);
        _authenticationService.HashPassword(command.NewPassword).Returns(newHashedPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.PasswordHash.Should().Be(newHashedPassword);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, userId)
            .Create();

        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("User not found.");

        _authenticationService.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, userId)
            .With(x => x.CurrentPassword, "WrongPassword123!")
            .Create();

        var user = new User("testuser", "test@example.com", "hashedCorrectPassword");

        _userRepository.GetByIdAsync(userId).Returns(user);
        _authenticationService.VerifyPassword(command.CurrentPassword, user.PasswordHash).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Current password is incorrect.");

        _authenticationService.DidNotReceive().HashPassword(Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallAuthenticationServiceToVerifyPassword_WhenHandlingCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, userId)
            .With(x => x.CurrentPassword, "CurrentPassword123!")
            .Create();

        var user = new User("testuser", "test@example.com", "hashedCurrentPassword");

        _userRepository.GetByIdAsync(userId).Returns(user);
        _authenticationService.VerifyPassword(command.CurrentPassword, user.PasswordHash).Returns(true);
        _authenticationService.HashPassword(command.NewPassword).Returns("hashedNewPassword");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authenticationService.Received(1).VerifyPassword("CurrentPassword123!", "hashedCurrentPassword");
    }

    [Fact]
    public async Task Handle_ShouldCallAuthenticationServiceToHashNewPassword_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, userId)
            .With(x => x.NewPassword, "NewPassword123!")
            .Create();

        var user = new User("testuser", "test@example.com", "hashedCurrentPassword");

        _userRepository.GetByIdAsync(userId).Returns(user);
        _authenticationService.VerifyPassword(command.CurrentPassword, user.PasswordHash).Returns(true);
        _authenticationService.HashPassword(command.NewPassword).Returns("hashedNewPassword");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authenticationService.Received(1).HashPassword(command.NewPassword);
    }

    [Fact]
    public async Task Handle_ShouldNotHashNewPassword_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = _fixture.Build<ChangePasswordCommand>()
            .With(x => x.UserId, userId)
            .Create();

        var user = new User("testuser", "test@example.com", "hashedCurrentPassword");

        _userRepository.GetByIdAsync(userId).Returns(user);
        _authenticationService.VerifyPassword(command.CurrentPassword, user.PasswordHash).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _authenticationService.DidNotReceive().HashPassword(Arg.Any<string>());
    }
}