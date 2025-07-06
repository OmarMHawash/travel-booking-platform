using AutoFixture;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;

namespace TravelBookingPlatform.Identity.IntegrationTests;

public class UserIntegrationTests : IdentityIntegrationTestBase
{
    private readonly Fixture _fixture;

    public UserIntegrationTests(IdentityIntegrationTestWebApplicationFactory factory) : base(factory)
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetProfile_ShouldReturnOkStatusCode_WhenUserIsAuthenticated()
    {
        // Arrange - Create a real user and authenticate with JWT
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"profileuser{uniqueId}")
            .With(x => x.Email, $"profileuser{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        // Register and login to get JWT token
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        // Act
        var response = await Client.GetAsync("/api/v1/User/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userProfile = await response.Content.ReadFromJsonAsync<UserDto>();
        userProfile.Should().NotBeNull();
        userProfile!.Username.Should().Be(registerCommand.Username);
        userProfile.Email.Should().Be(registerCommand.Email);
    }

    [Fact]
    public async Task GetUser_ShouldReturnUnauthorized_WhenUserIsNotAdmin()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"testuser{uniqueId}")
            .With(x => x.Email, $"testuser{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        // Register a user first (will have TypicalUser role by default)
        var registerResponse = await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var createdUser = await registerResponse.Content.ReadFromJsonAsync<UserDto>();

        // Login to get JWT token for authorization
        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        // Act - Try to access admin-only endpoint with TypicalUser role
        var response = await Client.GetAsync($"/api/v1/User/{createdUser!.Id}");

        // Assert - Should be forbidden since user doesn't have Admin role
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUser_ShouldReturnUnauthorized_WhenUserIsNotAdminForNonExistentUser()
    {
        // Arrange - Create and authenticate a regular user
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"regularuser{uniqueId}")
            .With(x => x.Email, $"regularuser{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        // Register and login to get JWT token
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        var nonExistentUserId = Guid.NewGuid();

        // Act - Try to access admin-only endpoint with TypicalUser role
        var response = await Client.GetAsync($"/api/v1/User/{nonExistentUserId}");

        // Assert - Should be forbidden since user doesn't have Admin role (even for non-existent users)
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnOkStatusCode_WhenValidPasswordChangeProvided()
    {
        // Arrange - Create and authenticate user
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"changepassuser{uniqueId}")
            .With(x => x.Email, $"changepassuser{uniqueId}@example.com")
            .With(x => x.Password, "OldPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        var changePasswordDto = _fixture.Build<ChangePasswordDto>()
            .With(x => x.CurrentPassword, registerCommand.Password)
            .With(x => x.NewPassword, "NewPassword123!")
            .Create();

        // Register and login to get JWT token
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange - Create and authenticate user first
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"invaliduser{uniqueId}")
            .With(x => x.Email, $"invaliduser{uniqueId}@example.com")
            .With(x => x.Password, "TestPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        var invalidChangePasswordDto = _fixture.Build<ChangePasswordDto>()
            .With(x => x.CurrentPassword, "") // Invalid empty current password
            .With(x => x.NewPassword, "") // Invalid empty new password
            .Create();

        // Register and login to get JWT token
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/User/change-password", invalidChangePasswordDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnUnauthorized_WhenWrongCurrentPasswordProvided()
    {
        // Arrange - Create and authenticate user
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"wrongcurrentpassuser{uniqueId}")
            .With(x => x.Email, $"wrongcurrentpassuser{uniqueId}@example.com")
            .With(x => x.Password, "CorrectPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        var changePasswordDto = _fixture.Build<ChangePasswordDto>()
            .With(x => x.CurrentPassword, "WrongCurrentPassword123!")
            .With(x => x.NewPassword, "NewPassword123!")
            .Create();

        // Register and login to get JWT token
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenNewPasswordDoesNotMeetComplexityRequirements()
    {
        // Arrange - Create and authenticate user
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"weakpassuser{uniqueId}")
            .With(x => x.Email, $"weakpassuser{uniqueId}@example.com")
            .With(x => x.Password, "StrongPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        var changePasswordDto = _fixture.Build<ChangePasswordDto>()
            .With(x => x.CurrentPassword, registerCommand.Password)
            .With(x => x.NewPassword, "weak") // Weak password
            .Create();

        // Register and login to get JWT token
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenNewPasswordSameAsCurrentPassword()
    {
        // Arrange - Create and authenticate user
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var samePassword = "SamePassword123!";
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"samepassuser{uniqueId}")
            .With(x => x.Email, $"samepassuser{uniqueId}@example.com")
            .With(x => x.Password, samePassword)
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        var changePasswordDto = _fixture.Build<ChangePasswordDto>()
            .With(x => x.CurrentPassword, samePassword)
            .With(x => x.NewPassword, samePassword) // Same as current password
            .Create();

        // Register and login to get JWT token
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_ShouldAllowLoginWithNewPassword_AfterSuccessfulPasswordChange()
    {
        // Arrange - Create and authenticate user
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerCommand = _fixture.Build<RegisterUserCommand>()
            .With(x => x.Username, $"newpassloginuser{uniqueId}")
            .With(x => x.Email, $"newpassloginuser{uniqueId}@example.com")
            .With(x => x.Password, "OldPassword123!")
            .Create();

        var loginCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, registerCommand.Password)
            .Create();

        var changePasswordDto = _fixture.Build<ChangePasswordDto>()
            .With(x => x.CurrentPassword, registerCommand.Password)
            .With(x => x.NewPassword, "NewPassword123!")
            .Create();

        var loginWithNewPasswordCommand = _fixture.Build<LoginCommand>()
            .With(x => x.Email, registerCommand.Email)
            .With(x => x.Password, changePasswordDto.NewPassword)
            .Create();

        // Register and login to get JWT token
        await Client.PostAsJsonAsync("/api/v1/Auth/register", registerCommand);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token.Token);

        // Change password
        await Client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordDto);

        // Clear auth header for new login
        Client.DefaultRequestHeaders.Authorization = null;

        // Act - Try to login with new password
        var response = await Client.PostAsJsonAsync("/api/v1/Auth/login", loginWithNewPasswordCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newAuthResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        newAuthResponse.Should().NotBeNull();
        newAuthResponse!.Token.Token.Should().NotBeNull();
        newAuthResponse.User.Email.Should().Be(registerCommand.Email);
    }
}