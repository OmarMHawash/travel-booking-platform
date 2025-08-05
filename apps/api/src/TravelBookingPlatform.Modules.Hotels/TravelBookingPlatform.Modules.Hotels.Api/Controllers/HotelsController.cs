using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Identity.Application.Services;

namespace TravelBookingPlatform.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IActivityTrackingService _activityTrackingService;
    private readonly ILogger<HotelsController> _logger;

    public HotelsController(IMediator mediator, IActivityTrackingService activityTrackingService, ILogger<HotelsController> logger)
    {
        _mediator = mediator;
        _activityTrackingService = activityTrackingService;
        _logger = logger;
    }

    /// <summary>
    /// Get hotel details by ID
    /// </summary>
    /// <param name="id">Hotel ID</param>
    /// <returns>Detailed hotel information including rooms, city, and pricing</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HotelDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHotelById(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid hotel ID provided.", nameof(id));
        }

        var query = new GetHotelByIdQuery(id);
        var hotel = await _mediator.Send(query);

        if (hotel == null)
        {
            throw new NotFoundException($"Hotel with ID {id} not found.");
        }

        // Track hotel view activity for authenticated users (non-blocking)
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            // Fire and forget - don't await to avoid impacting response time
            _ = Task.Run(async () =>
            {
                try
                {
                    await _activityTrackingService.TrackHotelViewAsync(userId.Value, id,
    $"Hotel: {hotel.Name} in {hotel.City.Name}");

                    _logger.LogDebug("Successfully tracked hotel view for user {UserId} viewing hotel {HotelId}",
                        userId.Value, id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to track hotel view activity for user {UserId} viewing hotel {HotelId}",
                        userId.Value, id);
                }
            });
        }
        else
        {
            _logger.LogDebug("Hotel {HotelId} viewed by anonymous user - no activity tracking", id);
        }

        return Ok(hotel);
    }

    /// <summary>
    /// Gets available room types for a given hotel, date range, and guest count.
    /// </summary>
    /// <param name="hotelId">The ID of the hotel.</param>
    /// <param name="checkInDate">The check-in date.</param>
    /// <param name="checkOutDate">The check-out date.</param>
    /// <param name="numberOfAdults">The number of adults.</param>
    /// <param name="numberOfChildren">The number of children.</param>
    /// <returns>A list of available room types with their details and availability count.</returns>
    [HttpGet("{hotelId}/availability")]
    [ProducesResponseType(typeof(List<AvailableRoomTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHotelRoomAvailability(
        [FromRoute] Guid hotelId,
        [FromQuery] DateTime checkInDate,
        [FromQuery] DateTime checkOutDate,
        [FromQuery] int numberOfAdults = 2,
        [FromQuery] int numberOfChildren = 0)
    {
        var query = new GetHotelRoomAvailabilityQuery(hotelId, checkInDate, checkOutDate, numberOfAdults, numberOfChildren);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Extracts the current user ID from the authentication claims
    /// </summary>
    /// <returns>User ID if authenticated, null otherwise</returns>
    private Guid? GetCurrentUserId()
    {
        if (!User.Identity?.IsAuthenticated == true)
            return null;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return null;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}