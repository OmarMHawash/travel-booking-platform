using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Application.Queries;
using TravelBookingPlatform.Modules.Identity.Application.Queries.Handlers;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;

namespace TravelBookingPlatform.Identity.UnitTests.Users.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly GetUserByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetUserByIdQueryHandlerTests()
    {
        _fixture = new Fixture();
        _userRepository = Substitute.For<IUserRepository>();

        // AutoMapper manual configuration for testing
        var mappingConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        }, NullLoggerFactory.Instance);
        var mapper = mappingConfig.CreateMapper();

        _handler = new GetUserByIdQueryHandler(_userRepository, mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = _fixture.Build<GetUserByIdQuery>()
            .With(x => x.UserId, userId)
            .Create();

        var user = new User("testuser", "test@example.com", "hashedPassword");

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email.Value);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDtoWithRandomData_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = _fixture.Build<GetUserByIdQuery>()
            .With(x => x.UserId, userId)
            .Create();

        var user = new User(_fixture.Create<string>(), _fixture.Create<string>() + "@example.com", _fixture.Create<string>());

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email.Value);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = _fixture.Build<GetUserByIdQuery>()
            .With(x => x.UserId, userId)
            .Create();

        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallUserRepository_WhenHandlingQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = _fixture.Build<GetUserByIdQuery>()
            .With(x => x.UserId, userId)
            .Create();

        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).GetByIdAsync(userId);
    }

    [Fact]
    public async Task Handle_ShouldMapUserPropertiesCorrectly_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = _fixture.Build<GetUserByIdQuery>()
            .With(x => x.UserId, userId)
            .Create();

        var user = new User("testuser", "test@example.com", "hashedPassword");

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email.Value);
        result.Role.Should().Be(user.Role);
        result.IsActive.Should().Be(user.IsActive);
        result.CreatedAt.Should().Be(user.CreatedAt);
        result.UpdatedAt.Should().Be(user.UpdatedAt);
    }
}