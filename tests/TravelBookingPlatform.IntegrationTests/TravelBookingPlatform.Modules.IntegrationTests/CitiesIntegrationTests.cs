using AutoFixture;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.IntegrationTests;

public class CitiesIntegrationTests : IntegrationTestBase
{
    private readonly Fixture _fixture;

    public CitiesIntegrationTests(IntegrationTestWebApplicationFactory factory) : base(factory)
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetAllCities_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/cities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCity_ShouldReturnCreatedStatusCode_WhenValidCityProvided()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Get 8 character hex string
        var cityNames = new[] { "Springfield", "Madison", "Franklin", "Georgetown", "Kingston", "Bristol", "Clinton", "Fairview" };
        var countries = new[] { "United States", "Canada", "United Kingdom", "Australia", "New Zealand", "Ireland" };
        var suffixes = new[] { "North", "South", "East", "West", "Central", "Upper", "Lower", "New" };

        var random = new Random();

        // Create unique postcode using only digits
        var uniquePostCode = $"{random.Next(10000, 99999)}"; // 5 digit postcode

        var createCityDto = new CreateCityDto
        {
            Name = $"{suffixes[random.Next(suffixes.Length)]} {cityNames[random.Next(cityNames.Length)]}", // Only letters and spaces
            Country = countries[random.Next(countries.Length)],
            PostCode = uniquePostCode
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", createCityDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdCity = await response.Content.ReadFromJsonAsync<CityDto>();
        createdCity.Should().NotBeNull();
        createdCity.Name.Should().Be(createCityDto.Name);
        createdCity.Country.Should().Be(createCityDto.Country);
        createdCity.PostCode.Should().Be(createCityDto.PostCode);
    }

    [Fact]
    public async Task CreateCity_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange - Test various invalid scenarios that should fail with new validation
        var invalidCreateCityDto = new CreateCityDto
        {
            Name = "1", // Too short and contains numbers
            Country = "USA123", // Contains numbers
            PostCode = "AB" // Too short and invalid format
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", invalidCreateCityDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}