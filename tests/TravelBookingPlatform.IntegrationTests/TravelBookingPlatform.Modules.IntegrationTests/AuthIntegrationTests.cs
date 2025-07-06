using AutoFixture;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;

namespace TravelBookingPlatform.Identity.IntegrationTests;

public class AuthIntegrationTests : IdentityIntegrationTestBase
{
    private readonly Fixture _fixture;

    public AuthIntegrationTests(IdentityIntegrationTestWebApplicationFactory factory) : base(factory)
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Register_ShouldReturnCreatedStatusCode_WhenValidUserProvided()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Use first 8 chars for uniqueness
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"testuser{uniqueId}")
            .With(x => x.Email, $"testuser{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
        createdUser.Should().NotBeNull();
        createdUser!.Username.Should().Be(registerCommand.Username);
        createdUser.Email.Should().Be(registerCommand.Email);
        createdUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange
        var invalidCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, "") // Invalid empty username
            .With(x => x.Email, "invalid-email") // Invalid email format
            .With(x => x.Password, "weak") // Weak password
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Auth/register", invalidCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var firstUserCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"firstuser{uniqueId}")
            .With(x => x.Email, $"duplicate{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        var duplicateEmailCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"seconduser{uniqueId}")
            .With(x => x.Email, $"duplicate{uniqueId}@example.com") // Same email
            .With(x => x.Password, "TestPassword123!")
            .Create();

        // Act
        var firstResponse = await Client.PostAsJsonAsync("/api/v1/Auth/register", firstUserCommand);
        var duplicateResponse = await Client.PostAsJsonAsync("/api/v1/Auth/register", duplicateEmailCommand);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var firstUserCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"duplicateuser{uniqueId}")
            .With(x => x.Email, $"first{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        var duplicateUsernameCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"duplicateuser{uniqueId}") // Same username
            .With(x => x.Email, $"second{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        // Act
        var firstResponse = await Client.PostAsJsonAsync("/api/v1/Auth/register", firstUserCommand);
        var duplicateResponse = await Client.PostAsJsonAsync("/api/v1/Auth/register", duplicateUsernameCommand);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnOkStatusCode_WhenValidCredentialsProvided()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"loginuser{uniqueId}")
            .With(x => x.Email, $"loginuser{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        // Register user first
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNull();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be(registerCommand.Email);
        authResponse.User.Username.Should().Be(registerCommand.Username);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentialsProvided()
    {
        // Arrange
        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "nonexistent@example.com")
            .With(x => x.Password, "WrongPassword123!")
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange
        var invalidLoginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, "") // Invalid empty email
            .With(x => x.Password, "") // Invalid empty password
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Auth/login", invalidLoginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenWrongPasswordProvided()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"wrongpassuser{uniqueId}")
            .With(x => x.Email, $"wrongpassuser{uniqueId}@example.com")
            .With(x => x.Password, "CorrectPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, "WrongPassword123!")
            .Create();

        // Register user first
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}