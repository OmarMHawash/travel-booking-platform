using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.Commands.Handlers;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Application.Services;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;

namespace TravelBookingPlatform.Identity.UnitTests.Users.Commands;

public class LoginCommandHandlerTests
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenService _tokenService;
    private readonly LoginCommandHandler _handler;
    private readonly Fixture _fixture;

    public LoginCommandHandlerTests()
    {
        _fixture = new Fixture();
        _authenticationService = Substitute.For<IAuthenticationService>();
        _tokenService = Substitute.For<ITokenService>();

        // AutoMapper manual configuration for testing
        var mappingConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        }, NullLoggerFactory.Instance);
        var mapper = mappingConfig.CreateMapper();

        _handler = new LoginCommandHandler(_authenticationService, _tokenService, mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResponseDto_WhenCredentialsAreValid()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "ValidPassword123!")
            .Create();

        var user = new User("testuser", command.Email, "hashedPassword");
        var token = _fixture.Build<TokenDto>()
            .With(x => x.Token, "valid_jwt_token")
            .Create();

        _authenticationService.AuthenticateAsync(command.Email, command.Password).Returns(user);
        _tokenService.GenerateToken(user).Returns(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(command.Email);
        result.User.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResponseWithRandomData_WhenCredentialsAreValid()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, _fixture.Create<string>() + "@example.com")
            .Create();

        var user = new User(_fixture.Create<string>(), command.Email, _fixture.Create<string>());
        var token = _fixture.Create<TokenDto>();

        _authenticationService.AuthenticateAsync(command.Email, command.Password).Returns(user);
        _tokenService.GenerateToken(user).Returns(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(command.Email);
        result.User.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenCredentialsAreInvalid()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "invalid@example.com")
            .With(x => x.Password, "wrongpassword")
            .Create();

        _authenticationService.AuthenticateAsync(command.Email, command.Password).Returns((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsInactive()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "ValidPassword123!")
            .Create();

        var user = new User("testuser", command.Email, "hashedPassword");
        user.Deactivate(); // Make user inactive

        _authenticationService.AuthenticateAsync(command.Email, command.Password).Returns(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("User account is deactivated.");
    }

    [Fact]
    public async Task Handle_ShouldCallAuthenticationService_WhenHandlingCommand()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "ValidPassword123!")
            .Create();

        var user = new User("testuser", command.Email, "hashedPassword");
        var token = _fixture.Create<TokenDto>();

        _authenticationService.AuthenticateAsync(command.Email, command.Password).Returns(user);
        _tokenService.GenerateToken(user).Returns(token);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _authenticationService.Received(1).AuthenticateAsync(command.Email, command.Password);
    }

    [Fact]
    public async Task Handle_ShouldCallTokenService_WhenUserIsAuthenticated()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "ValidPassword123!")
            .Create();

        var user = new User("testuser", command.Email, "hashedPassword");
        var token = _fixture.Create<TokenDto>();

        _authenticationService.AuthenticateAsync(command.Email, command.Password).Returns(user);
        _tokenService.GenerateToken(user).Returns(token);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _tokenService.Received(1).GenerateToken(user);
    }

    [Fact]
    public async Task Handle_ShouldNotCallTokenService_WhenAuthenticationFails()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "invalid@example.com")
            .With(x => x.Password, "wrongpassword")
            .Create();

        _authenticationService.AuthenticateAsync(command.Email, command.Password).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _tokenService.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}