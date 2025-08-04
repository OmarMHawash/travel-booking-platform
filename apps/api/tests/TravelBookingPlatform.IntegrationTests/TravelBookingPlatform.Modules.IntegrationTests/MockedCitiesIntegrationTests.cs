using AutoFixture;
using FluentAssertions;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.IntegrationTests.Helpers;

namespace TravelBookingPlatform.Modules.IntegrationTests;

public class MockedCitiesIntegrationTests : MockedIntegrationTestBase
{
    public MockedCitiesIntegrationTests(MockedIntegrationTestWebApplicationFactory factory) : base(factory)
    {
    }

    private void ResetMockState()
    {
        // Clear all mock call history to prevent interference between tests
        MockCityRepository.ClearReceivedCalls();
        MockUnitOfWork.ClearReceivedCalls();

        // Reset all mock return values to default safe state
        // Note: Later setups in individual tests will override these defaults
        MockCityRepository.NameExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);
        MockCityRepository.PostCodeExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);
        MockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task GetAllCities_ShouldReturnSuccessStatusCode_WhenCitiesExist()
    {
        // Arrange
        ResetMockState();
        var cities = TestDataBuilders.CreateValidCities(3);
        MockCityRepository.GetAllAsync().Returns(cities.AsReadOnly());

        // Act
        var response = await Client.GetAsync("/api/v1/cities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Verify repository was called
        await MockCityRepository.Received().GetAllAsync();
    }

    [Fact]
    public async Task GetAllCities_ShouldReturnEmptyList_WhenNoCitiesExist()
    {
        // Arrange
        ResetMockState();
        MockCityRepository.GetAllAsync().Returns(new List<City>().AsReadOnly());

        // Act
        var response = await Client.GetAsync("/api/v1/cities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCity_ShouldReturnCreatedStatusCode_WhenValidCityProvided()
    {
        // Arrange
        ResetMockState();
        var createCityCommand = TestDataBuilders.CreateValidCityCommand();

        // Setup repository mocks to allow creation (no conflicts)
        MockCityRepository.NameExistsAsync(createCityCommand.Name, Arg.Any<Guid?>()).Returns(false);
        MockCityRepository.PostCodeExistsAsync(createCityCommand.PostCode, Arg.Any<Guid?>()).Returns(false);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", createCityCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify the repository interactions
        await MockCityRepository.Received(1).NameExistsAsync(createCityCommand.Name, Arg.Any<Guid?>());
        await MockCityRepository.Received(1).PostCodeExistsAsync(createCityCommand.PostCode, Arg.Any<Guid?>());
        await MockCityRepository.Received(1).AddAsync(Arg.Is<City>(c =>
            c.Name == createCityCommand.Name &&
            c.Country == createCityCommand.Country &&
            c.PostCode == createCityCommand.PostCode));
        await MockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        // Verify response content
        var createdCity = await response.Content.ReadFromJsonAsync<CityDto>();
        createdCity.Should().NotBeNull();
        createdCity.Name.Should().Be(createCityCommand.Name);
        createdCity.Country.Should().Be(createCityCommand.Country);
        createdCity.PostCode.Should().Be(createCityCommand.PostCode);
    }

    [Fact]
    public async Task CreateCity_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange
        ResetMockState();
        var invalidCreateCityCommand = TestDataBuilders.CreateInvalidCityCommand();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", invalidCreateCityCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no repository interactions occurred (validation failed before reaching repository)
        await MockCityRepository.DidNotReceive().AddAsync(Arg.Any<City>());
        await MockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCity_ShouldReturnBadRequest_WhenCityNameAlreadyExists()
    {
        // Arrange
        ResetMockState();
        var createCityCommand = TestDataBuilders.CreateValidCityCommand();

        // Setup repository mock to simulate name conflict
        MockCityRepository.NameExistsAsync(createCityCommand.Name, Arg.Any<Guid?>()).Returns(true);
        MockCityRepository.PostCodeExistsAsync(createCityCommand.PostCode, Arg.Any<Guid?>()).Returns(false);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", createCityCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify only the existence check was called, but not the add operation
        await MockCityRepository.Received(1).NameExistsAsync(createCityCommand.Name, Arg.Any<Guid?>());
        await MockCityRepository.DidNotReceive().AddAsync(Arg.Any<City>());
        await MockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCity_ShouldReturnBadRequest_WhenPostCodeAlreadyExists()
    {
        // Arrange
        ResetMockState();
        var createCityCommand = TestDataBuilders.CreateCityCommandWithPostCode("12345");

        // Setup repository mocks
        MockCityRepository.NameExistsAsync(createCityCommand.Name, Arg.Any<Guid?>()).Returns(false);
        MockCityRepository.PostCodeExistsAsync("12345", Arg.Any<Guid?>()).Returns(true);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", createCityCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify both existence checks were called, but not the add operation
        await MockCityRepository.Received(1).NameExistsAsync(createCityCommand.Name, Arg.Any<Guid?>());
        await MockCityRepository.Received(1).PostCodeExistsAsync("12345", Arg.Any<Guid?>());
        await MockCityRepository.DidNotReceive().AddAsync(Arg.Any<City>());
        await MockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCity_ShouldHandleRepositoryFailure_WhenSaveChangesFails()
    {
        // Arrange - Use ONLY letters for validation compliance (NO numbers at all!)
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();
        var randomSuffix = new string(Enumerable.Range(0, 6).Select(_ => letters[random.Next(letters.Length)]).ToArray());

        var createCityCommand = new CreateCityCommand
        {
            Name = $"TestFailureCity-{randomSuffix}",      // Pure letters only: TestFailureCity-ABCDEF
            Country = $"TestCountry-{randomSuffix}",       // Pure letters only: TestCountry-ABCDEF  
            PostCode = $"{Random.Shared.Next(10000, 99999)}"  // Numbers OK in postcode
        };

        // NUCLEAR APPROACH: Override ALL possible mock setups with broad patterns
        MockCityRepository.ClearReceivedCalls();
        MockUnitOfWork.ClearReceivedCalls();

        // Override ANY name/postcode check to return false (no conflicts)
        MockCityRepository.NameExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);
        MockCityRepository.PostCodeExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);

        // Force repository failure AFTER all validation passes - use Exception type that maps to 500
        MockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", createCityCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        // Verify all operations were attempted
        await MockCityRepository.Received(1).AddAsync(Arg.Any<City>());
        await MockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCity_ShouldHandleAutoFixtureGeneratedData()
    {
        // Arrange - Use ONLY letters for validation compliance (NO numbers at all!)
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();
        var randomSuffix1 = new string(Enumerable.Range(0, 6).Select(_ => letters[random.Next(letters.Length)]).ToArray());
        var randomSuffix2 = new string(Enumerable.Range(0, 6).Select(_ => letters[random.Next(letters.Length)]).ToArray());

        var createCityCommand = new CreateCityCommand
        {
            Name = $"AutoTestCity-{randomSuffix1}",         // Pure letters only: AutoTestCity-ABCDEF
            Country = $"AutoTestCountry-{randomSuffix2}",   // Pure letters only: AutoTestCountry-GHIJKL
            PostCode = $"{Random.Shared.Next(50000, 99999)}"  // Numbers OK in postcode
        };

        // NUCLEAR APPROACH: Override ALL possible mock setups with broad patterns
        MockCityRepository.ClearReceivedCalls();
        MockUnitOfWork.ClearReceivedCalls();

        // Override ANY name/postcode check to return false (no conflicts) 
        MockCityRepository.NameExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);
        MockCityRepository.PostCodeExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);
        MockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", createCityCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdCity = await response.Content.ReadFromJsonAsync<CityDto>();
        createdCity.Should().NotBeNull();
        createdCity.Name.Should().Be(createCityCommand.Name);
        createdCity.Country.Should().Be(createCityCommand.Country);
        createdCity.PostCode.Should().Be(createCityCommand.PostCode);
    }
}