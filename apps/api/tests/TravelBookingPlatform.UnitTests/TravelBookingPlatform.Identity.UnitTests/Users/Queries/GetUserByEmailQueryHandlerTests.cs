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

public class GetUserByEmailQueryHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly GetUserByEmailQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetUserByEmailQueryHandlerTests()
    {
        _fixture = new Fixture();
        _userRepository = Substitute.For<IUserRepository>();

        // AutoMapper manual configuration for testing
        var mappingConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        }, NullLoggerFactory.Instance);
        var mapper = mappingConfig.CreateMapper();

        _handler = new GetUserByEmailQueryHandler(_userRepository, mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var query = _fixture.Build<GetUserByEmailQuery>()
            .With(x => x.Email, email)
            .Create();

        var user = new User("testuser", email, "hashedPassword");

        _userRepository.GetByEmailAsync(email).Returns(user);

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
        var email = _fixture.Create<string>() + "@example.com";
        var query = _fixture.Build<GetUserByEmailQuery>()
            .With(x => x.Email, email)
            .Create();

        var user = new User(_fixture.Create<string>(), email, _fixture.Create<string>());

        _userRepository.GetByEmailAsync(email).Returns(user);

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
        var email = "nonexistent@example.com";
        var query = _fixture.Build<GetUserByEmailQuery>()
            .With(x => x.Email, email)
            .Create();

        _userRepository.GetByEmailAsync(email).Returns((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallUserRepository_WhenHandlingQuery()
    {
        // Arrange
        var email = "test@example.com";
        var query = _fixture.Build<GetUserByEmailQuery>()
            .With(x => x.Email, email)
            .Create();

        _userRepository.GetByEmailAsync(email).Returns((User?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).GetByEmailAsync(email);
    }

    [Fact]
    public async Task Handle_ShouldMapUserPropertiesCorrectly_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var query = _fixture.Build<GetUserByEmailQuery>()
            .With(x => x.Email, email)
            .Create();

        var user = new User("testuser", email, "hashedPassword");

        _userRepository.GetByEmailAsync(email).Returns(user);

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

    [Theory]
    [InlineData("user1@example.com")]
    [InlineData("user2@domain.org")]
    [InlineData("admin@company.co.uk")]
    public async Task Handle_ShouldReturnCorrectUser_ForDifferentEmailAddresses(string email)
    {
        // Arrange
        var query = _fixture.Build<GetUserByEmailQuery>()
            .With(x => x.Email, email)
            .Create();

        var user = new User(_fixture.Create<string>(), email, _fixture.Create<string>());

        _userRepository.GetByEmailAsync(email).Returns(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Username.Should().Be(user.Username);
    }
}