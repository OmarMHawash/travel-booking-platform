using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.Commands.Handlers;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Application.Services;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;

namespace TravelBookingPlatform.Identity.UnitTests.Users.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;
    private readonly RegisterUserCommandHandler _handler;
    private readonly Fixture _fixture;

    public RegisterUserCommandHandlerTests()
    {
        _fixture = new Fixture();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _authenticationService = Substitute.For<IAuthenticationService>();

        // AutoMapper manual configuration for testing
        var mappingConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        }, NullLoggerFactory.Instance);
        var mapper = mappingConfig.CreateMapper();

        _handler = new RegisterUserCommandHandler(_userRepository, _unitOfWork, mapper, _authenticationService);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAndReturnUserDto_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "testuser")
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        var hashedPassword = "hashed_password";

        _userRepository.EmailExistsAsync(command.Email).Returns(false);
        _userRepository.UsernameExistsAsync(command.Username).Returns(false);
        _authenticationService.HashPassword(command.Password).Returns(hashedPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u =>
            u.Username == command.Username &&
            u.Email.Value == command.Email &&
            u.PasswordHash == hashedPassword));

        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Username.Should().Be(command.Username);
        result.Email.Should().Be(command.Email);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserWithRandomData_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Email, _fixture.Create<string>() + "@example.com")
            .Create();

        var hashedPassword = _fixture.Create<string>();

        _userRepository.EmailExistsAsync(Arg.Any<string>()).Returns(false);
        _userRepository.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        _authenticationService.HashPassword(Arg.Any<string>()).Returns(hashedPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u =>
            u.Username == command.Username &&
            u.Email.Value == command.Email &&
            u.PasswordHash == hashedPassword));

        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Username.Should().Be(command.Username);
        result.Email.Should().Be(command.Email);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Email, "existing@example.com")
            .Create();

        _userRepository.EmailExistsAsync(command.Email).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Email already exists.");

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUsernameAlreadyExists()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "existinguser")
            .With(x => x.Email, "test@example.com")
            .Create();

        _userRepository.EmailExistsAsync(Arg.Any<string>()).Returns(false);
        _userRepository.UsernameExistsAsync(command.Username).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Username already exists.");

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallAuthenticationServiceToHashPassword_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        var hashedPassword = "hashed_password_result";

        _userRepository.EmailExistsAsync(Arg.Any<string>()).Returns(false);
        _userRepository.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        _authenticationService.HashPassword(command.Password).Returns(hashedPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authenticationService.Received(1).HashPassword(command.Password);
    }
}