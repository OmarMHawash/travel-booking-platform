using FluentAssertions;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.IntegrationTests.Helpers;

namespace TravelBookingPlatform.Modules.IntegrationTests;

public class MockedHotelsIntegrationTests : MockedIntegrationTestBase
{
    public MockedHotelsIntegrationTests(MockedIntegrationTestWebApplicationFactory factory) : base(factory)
    {
    }

    private void ResetMockState()
    {
        // Clear all mock call history to prevent interference between tests
        MockHotelRepository.ClearReceivedCalls();
        MockUnitOfWork.ClearReceivedCalls();

        // Reset all mock return values to default safe state
        MockHotelRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Hotel?)null);
        MockHotelRepository.GetHotelWithDetailsAsync(Arg.Any<Guid>()).Returns((Hotel?)null);
        MockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    #region GET /api/v1/hotels/{id}

    [Fact]
    public async Task GetHotelById_ShouldReturnSuccessStatusCode_WhenHotelExists()
    {
        // Arrange
        ResetMockState();
        var hotelId = Guid.NewGuid();
        var hotel = CreateHotelWithDetails(hotelId);
        MockHotelRepository.GetHotelWithDetailsAsync(hotelId).Returns(hotel);

        // Act
        var response = await Client.GetAsync($"/api/v1/hotels/{hotelId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<HotelDetailDto>();
        result.Should().NotBeNull();
        result.Id.Should().Be(hotelId);
        result.Name.Should().Be(hotel.Name);
        result.Description.Should().Be(hotel.Description);
        result.Rating.Should().Be(hotel.Rating);
        result.City.Should().NotBeNull();
        result.Rooms.Should().NotBeEmpty();
        result.TotalRooms.Should().BeGreaterThan(0);
        result.MinPrice.Should().BeGreaterThan(0);
        result.MaxPrice.Should().BeGreaterThan(0);
        result.AvailableRoomTypes.Should().NotBeEmpty();

        // Verify repository was called
        await MockHotelRepository.Received().GetHotelWithDetailsAsync(hotelId);
    }

    [Fact]
    public async Task GetHotelById_ShouldReturnNotFound_WhenHotelDoesNotExist()
    {
        // Arrange
        ResetMockState();
        var hotelId = Guid.NewGuid();
        MockHotelRepository.GetHotelWithDetailsAsync(hotelId).Returns((Hotel?)null);

        // Act
        var response = await Client.GetAsync($"/api/v1/hotels/{hotelId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify repository was called
        await MockHotelRepository.Received().GetHotelWithDetailsAsync(hotelId);
    }

    [Fact]
    public async Task GetHotelById_ShouldReturnBadRequest_WhenInvalidGuidProvided()
    {
        // Arrange
        ResetMockState();
        var invalidGuid = "invalid-guid";

        // Act
        var response = await Client.GetAsync($"/api/v1/hotels/{invalidGuid}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository was not called due to invalid GUID
        await MockHotelRepository.DidNotReceive().GetHotelWithDetailsAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task GetHotelById_ShouldReturnBadRequest_WhenEmptyGuidProvided()
    {
        // Arrange
        ResetMockState();
        var emptyGuid = Guid.Empty;

        // Act
        var response = await Client.GetAsync($"/api/v1/hotels/{emptyGuid}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository was not called due to empty GUID
        await MockHotelRepository.DidNotReceive().GetHotelWithDetailsAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task GetHotelById_ShouldIncludeCompleteHotelStructure_WhenHotelWithRoomsExists()
    {
        // Arrange
        ResetMockState();
        var hotelId = Guid.NewGuid();
        var hotel = CreateHotelWithMultipleRoomsAndTypes(hotelId);
        MockHotelRepository.GetHotelWithDetailsAsync(hotelId).Returns(hotel);

        // Act
        var response = await Client.GetAsync($"/api/v1/hotels/{hotelId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<HotelDetailDto>();
        result.Should().NotBeNull();

        // Verify complete structure
        result.City.Should().NotBeNull();
        result.City.Name.Should().NotBeNullOrEmpty();
        result.City.Country.Should().NotBeNullOrEmpty();

        result.Rooms.Should().HaveCount(hotel.Rooms.Count);
        result.Rooms.Should().OnlyContain(r => !string.IsNullOrEmpty(r.RoomNumber));
        result.Rooms.Should().OnlyContain(r => r.RoomType != null);
        result.Rooms.Should().OnlyContain(r => r.RoomType.PricePerNight > 0);

        // Verify summary statistics
        result.TotalRooms.Should().Be(hotel.Rooms.Count);
        var expectedMinPrice = hotel.Rooms.Min(r => r.RoomType.PricePerNight);
        var expectedMaxPrice = hotel.Rooms.Max(r => r.RoomType.PricePerNight);
        result.MinPrice.Should().Be(expectedMinPrice);
        result.MaxPrice.Should().Be(expectedMaxPrice);

        var uniqueRoomTypes = hotel.Rooms.Select(r => r.RoomType.Name).Distinct().ToList();
        result.AvailableRoomTypes.Should().BeEquivalentTo(uniqueRoomTypes);
    }

    [Fact]
    public async Task GetHotelById_ShouldHandleHotelWithNoRooms_WhenHotelExistsButEmpty()
    {
        // Arrange
        ResetMockState();
        var hotelId = Guid.NewGuid();
        var hotel = CreateHotelWithoutRooms(hotelId);
        MockHotelRepository.GetHotelWithDetailsAsync(hotelId).Returns(hotel);

        // Act
        var response = await Client.GetAsync($"/api/v1/hotels/{hotelId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<HotelDetailDto>();
        result.Should().NotBeNull();
        result.Rooms.Should().BeEmpty();
        result.TotalRooms.Should().Be(0);
        result.MinPrice.Should().BeNull();
        result.MaxPrice.Should().BeNull();
        result.AvailableRoomTypes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHotelById_ShouldReturnInternalServerError_WhenRepositoryThrowsException()
    {
        // Arrange
        ResetMockState();
        var hotelId = Guid.NewGuid();
        MockHotelRepository.GetHotelWithDetailsAsync(hotelId)
            .Returns(Task.FromException<Hotel?>(new Exception("Database connection error")));

        // Act
        var response = await Client.GetAsync($"/api/v1/hotels/{hotelId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        // Verify repository was called
        await MockHotelRepository.Received().GetHotelWithDetailsAsync(hotelId);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a hotel with basic room and city details for testing
    /// </summary>
    private Hotel CreateHotelWithDetails(Guid hotelId)
    {
        var city = TestDataBuilders.CreateValidCity();
        var hotel = new Hotel(
            "Test Hotel",
            "Test Hotel Description",
            4.5m,
            city.Id);

        // Set the ID using reflection since it's likely protected
        typeof(Hotel).GetProperty("Id")?.SetValue(hotel, hotelId);

        // Add city relationship
        hotel.GetType().GetProperty("City")?.SetValue(hotel, city);

        // Add some rooms with room types
        var roomType1 = new RoomType("Standard Room", "Standard Room Description", 100m, 2, 1);
        var roomType2 = new RoomType("Deluxe Room", "Deluxe Room Description", 200m, 3, 2);

        var room1 = new Room("101", hotel.Id, roomType1.Id);
        var room2 = new Room("102", hotel.Id, roomType2.Id);

        // Set room type relationships
        room1.GetType().GetProperty("RoomType")?.SetValue(room1, roomType1);
        room2.GetType().GetProperty("RoomType")?.SetValue(room2, roomType2);

        // Add rooms to hotel (assuming there's a Rooms property)
        var roomsList = new List<Room> { room1, room2 };
        hotel.GetType().GetProperty("Rooms")?.SetValue(hotel, roomsList);

        return hotel;
    }

    /// <summary>
    /// Creates a hotel with multiple rooms and room types for comprehensive testing
    /// </summary>
    private Hotel CreateHotelWithMultipleRoomsAndTypes(Guid hotelId)
    {
        var city = TestDataBuilders.CreateValidCity();
        var hotel = new Hotel(
            "Grand Test Hotel",
            "Luxury test hotel with multiple room types",
            4.8m,
            city.Id);

        typeof(Hotel).GetProperty("Id")?.SetValue(hotel, hotelId);
        hotel.GetType().GetProperty("City")?.SetValue(hotel, city);

        // Create multiple room types
        var standardRoomType = new RoomType("Standard Room", "Standard Room Description", 150m, 2, 0);
        var deluxeRoomType = new RoomType("Deluxe Room", "Deluxe Room Description", 250m, 2, 1);
        var suiteRoomType = new RoomType("Executive Suite", "Executive Suite Description", 500m, 4, 2);

        // Create multiple rooms
        var rooms = new List<Room>
        {
            new Room("101", hotel.Id, standardRoomType.Id),
            new Room("102", hotel.Id, standardRoomType.Id),
            new Room("201", hotel.Id, deluxeRoomType.Id),
            new Room("202", hotel.Id, deluxeRoomType.Id),
            new Room("301", hotel.Id, suiteRoomType.Id)
        };

        // Set room type relationships
        rooms[0].GetType().GetProperty("RoomType")?.SetValue(rooms[0], standardRoomType);
        rooms[1].GetType().GetProperty("RoomType")?.SetValue(rooms[1], standardRoomType);
        rooms[2].GetType().GetProperty("RoomType")?.SetValue(rooms[2], deluxeRoomType);
        rooms[3].GetType().GetProperty("RoomType")?.SetValue(rooms[3], deluxeRoomType);
        rooms[4].GetType().GetProperty("RoomType")?.SetValue(rooms[4], suiteRoomType);

        hotel.GetType().GetProperty("Rooms")?.SetValue(hotel, rooms);

        return hotel;
    }

    /// <summary>
    /// Creates a hotel without any rooms for edge case testing
    /// </summary>
    private Hotel CreateHotelWithoutRooms(Guid hotelId)
    {
        var city = TestDataBuilders.CreateValidCity();
        var hotel = new Hotel(
            "Empty Test Hotel",
            "Test hotel with no rooms",
            3.5m,
            city.Id);

        typeof(Hotel).GetProperty("Id")?.SetValue(hotel, hotelId);
        hotel.GetType().GetProperty("City")?.SetValue(hotel, city);
        hotel.GetType().GetProperty("Rooms")?.SetValue(hotel, new List<Room>());

        return hotel;
    }

    #endregion
}