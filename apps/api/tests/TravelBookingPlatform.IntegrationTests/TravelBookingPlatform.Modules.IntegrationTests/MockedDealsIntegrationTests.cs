using AutoFixture;
using FluentAssertions;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.IntegrationTests.Helpers;

namespace TravelBookingPlatform.Modules.IntegrationTests;

public class MockedDealsIntegrationTests : MockedIntegrationTestBase
{
    public MockedDealsIntegrationTests(MockedIntegrationTestWebApplicationFactory factory) : base(factory)
    {
    }

    private void ResetMockState()
    {
        // Clear all mock call history to prevent interference between tests
        MockDealRepository.ClearReceivedCalls();
        MockHotelRepository.ClearReceivedCalls();
        MockRoomTypeRepository.ClearReceivedCalls();
        MockUnitOfWork.ClearReceivedCalls();

        // Reset all mock return values to default safe state
        MockDealRepository.GetFeaturedDealsAsync(Arg.Any<int>()).Returns(new List<Deal>());
        MockDealRepository.GetActiveDealsAsync().Returns(Task.FromResult(new List<Deal>())); // FIX: Use GetActiveDealsAsync instead of GetAllAsync
        MockDealRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Deal?)null);
        MockDealRepository.GetDealWithDetailsAsync(Arg.Any<Guid>()).Returns((Deal?)null);
        MockDealRepository.GetOverlappingDealAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((Deal?)null);
        MockDealRepository.GetByHotelAndTitleAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns((Deal?)null);

        // CRITICAL: Mock Hotel and RoomType repositories to return valid entities (CreateDealCommandHandler requires these)
        var mockHotel = TestDataBuilders.CreateValidHotel();
        var mockRoomType = TestDataBuilders.CreateValidRoomType();
        MockHotelRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(mockHotel);
        MockRoomTypeRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(mockRoomType);

        MockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    #region GET /api/v1/deals/featured

    [Fact]
    public async Task GetFeaturedDeals_ShouldReturnSuccessStatusCode_WhenFeaturedDealsExist()
    {
        // Arrange
        ResetMockState();
        var featuredDeals = TestDataBuilders.CreateFeaturedDeals(3);
        MockDealRepository.GetFeaturedDealsAsync(10).Returns(featuredDeals);

        // Act - Use default count parameter that's valid (10)
        var response = await Client.GetAsync("/api/v1/deals/featured?count=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<FeaturedDealDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Verify repository was called
        await MockDealRepository.Received().GetFeaturedDealsAsync(10);
    }

    [Fact]
    public async Task GetFeaturedDeals_ShouldReturnBadRequest_WhenCountIsInvalid()
    {
        // Arrange
        ResetMockState();

        // Act - count=0 is invalid (should be between 1 and 50)
        var response = await Client.GetAsync("/api/v1/deals/featured?count=0");

        // Assert - Model binding validation catches invalid parameters and returns 400 BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository was not called due to validation failure
        await MockDealRepository.DidNotReceive().GetFeaturedDealsAsync(Arg.Any<int>());
    }

    #endregion

    #region GET /api/v1/deals/{id}

    [Fact]
    public async Task GetDealById_ShouldReturnSuccessStatusCode_WhenDealExists()
    {
        // Arrange
        ResetMockState();
        var dealId = Guid.NewGuid();
        var deal = TestDataBuilders.CreateValidDeal();
        MockDealRepository.GetDealWithDetailsAsync(dealId).Returns(deal);

        // Act
        var response = await Client.GetAsync($"/api/v1/deals/{dealId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DealDto>();
        result.Should().NotBeNull();

        // Verify repository was called
        await MockDealRepository.Received().GetDealWithDetailsAsync(dealId);
    }

    [Fact]
    public async Task GetDealById_ShouldReturnNotFound_WhenDealDoesNotExist()
    {
        // Arrange
        ResetMockState();
        var dealId = Guid.NewGuid();
        MockDealRepository.GetDealWithDetailsAsync(dealId).Returns((Deal?)null);

        // Act
        var response = await Client.GetAsync($"/api/v1/deals/{dealId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify repository was called
        await MockDealRepository.Received().GetDealWithDetailsAsync(dealId);
    }

    #endregion

    #region GET /api/v1/deals (Admin Only)

    [Fact]
    public async Task GetAllDeals_ShouldReturnSuccessStatusCode_WhenAdminRequestsDeals()
    {
        // Arrange
        ResetMockState();
        var deals = TestDataBuilders.CreateValidDeals(5);
        MockDealRepository.GetActiveDealsAsync().Returns(Task.FromResult(deals)); // FIX: Use GetActiveDealsAsync

        // Act - Use valid pagination parameters (page=1, pageSize=10)
        var response = await Client.GetAsync("/api/v1/deals?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<DealDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(5);

        // Verify repository was called
        await MockDealRepository.Received().GetActiveDealsAsync(); // FIX: Use GetActiveDealsAsync
    }

    [Fact]
    public async Task GetAllDeals_ShouldReturnBadRequest_WhenPaginationIsInvalid()
    {
        // Arrange
        ResetMockState();

        // Act - page=0 is invalid (should be >= 1)
        var response = await Client.GetAsync("/api/v1/deals?page=0");

        // Assert - Model binding validation catches invalid parameters and returns 400 BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository was not called due to validation failure
        await MockDealRepository.DidNotReceive().GetActiveDealsAsync(); // FIX: Use GetActiveDealsAsync
    }

    #endregion

    #region POST /api/v1/deals (Admin Only)

    [Fact]
    public async Task CreateDeal_ShouldReturnCreatedStatusCode_WhenValidDealProvided()
    {
        // Arrange
        ResetMockState();
        var createDealDto = TestDataBuilders.CreateValidCreateDealDto();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/deals", createDealDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify repository interactions
        await MockDealRepository.Received(1).AddAsync(Arg.Is<Deal>(d =>
            d.Title == createDealDto.Title &&
            d.Description == createDealDto.Description &&
            d.HotelId == createDealDto.HotelId));
        await MockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        // Verify response contains the created deal ID
        var result = await response.Content.ReadFromJsonAsync<Guid>();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateDeal_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange
        ResetMockState();
        var invalidDealDto = TestDataBuilders.CreateInvalidCreateDealDto();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/deals", invalidDealDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no repository interactions occurred (validation failed before reaching repository)
        await MockDealRepository.DidNotReceive().AddAsync(Arg.Any<Deal>());
        await MockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDeal_ShouldReturnInternalServerError_WhenRepositoryFails()
    {
        // Arrange
        ResetMockState();
        var createDealDto = TestDataBuilders.CreateValidCreateDealDto();

        // Force repository failure AFTER all validation passes
        MockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database connection failed")));

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/deals", createDealDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        // Verify all operations were attempted
        await MockDealRepository.Received(1).AddAsync(Arg.Any<Deal>());
        await MockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region PUT /api/v1/deals/{id} (Admin Only)

    [Fact]
    public async Task UpdateDeal_ShouldReturnNoContent_WhenValidDealProvided()
    {
        // Arrange
        ResetMockState();
        var dealId = Guid.NewGuid();
        var existingDeal = TestDataBuilders.CreateValidDeal();
        var updateDealDto = TestDataBuilders.CreateValidUpdateDealDto();

        // CRITICAL: Mock GetByIdAsync to return existing deal (UpdateDealCommandHandler calls this first)
        MockDealRepository.GetByIdAsync(dealId).Returns(existingDeal);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/deals/{dealId}", updateDealDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update operations were called
        await MockDealRepository.Received(1).GetByIdAsync(dealId);
        await MockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDeal_ShouldReturnNotFound_WhenDealDoesNotExist()
    {
        // Arrange
        ResetMockState();
        var dealId = Guid.NewGuid();
        var updateDealDto = TestDataBuilders.CreateValidUpdateDealDto();

        // Setup repository to return null (deal not found)
        MockDealRepository.GetByIdAsync(dealId).Returns((Deal?)null);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/deals/{dealId}", updateDealDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify repository was queried but no update occurred
        await MockDealRepository.Received(1).GetByIdAsync(dealId);
        await MockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region PATCH /api/v1/deals/{id}/toggle-featured (Admin Only)

    [Fact]
    public async Task ToggleFeatured_ShouldReturnNoContent_WhenDealExists()
    {
        // Arrange
        ResetMockState();
        var dealId = Guid.NewGuid();
        var existingDeal = TestDataBuilders.CreateValidDeal();

        // CRITICAL: Mock GetByIdAsync to return existing deal (ToggleFeaturedDealCommandHandler calls this first)
        MockDealRepository.GetByIdAsync(dealId).Returns(existingDeal);

        // Act
        var response = await Client.PatchAsync($"/api/v1/deals/{dealId}/toggle-featured", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the toggle operations were called
        await MockDealRepository.Received(1).GetByIdAsync(dealId);
        await MockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ToggleFeatured_ShouldReturnNotFound_WhenDealDoesNotExist()
    {
        // Arrange
        ResetMockState();
        var dealId = Guid.NewGuid();

        // Setup repository to return null (deal not found)
        MockDealRepository.GetByIdAsync(dealId).Returns((Deal?)null);

        // Act
        var response = await Client.PatchAsync($"/api/v1/deals/{dealId}/toggle-featured", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify repository was queried but no update occurred
        await MockDealRepository.Received(1).GetByIdAsync(dealId);
        await MockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}