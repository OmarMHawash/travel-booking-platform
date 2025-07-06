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
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Use first 8 chars of GUID for uniqueness
        var createCityDto = _fixture.Build<CreateCityDto>()
            .With(x => x.Name, $"Test City {uniqueId}")
            .With(x => x.Country, $"Test Country {uniqueId}")
            .With(x => x.PostCode, $"{uniqueId}")
            .Create();

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
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var invalidCreateCityDto = _fixture.Build<CreateCityDto>()
            .With(x => x.Name, "") // Invalid empty name
            .With(x => x.Country, $"Test Country {uniqueId}")
            .With(x => x.PostCode, $"{uniqueId}")
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/cities", invalidCreateCityDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}