using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TravelBookingPlatform.Modules.Hotels.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddHotelsApi(this IServiceCollection services)
    {
        // Any module-specific API registrations (e.g., custom formatters, policies)
        // For now, this module mainly exposes controllers which are discovered by the Host.
        // This method serves as a placeholder for future API-specific DI.
        return services;
    }

    public static IApplicationBuilder UseHotelsApi(this IApplicationBuilder app)
    {
        // Any module-specific API middleware (e.g., custom authentication per module)
        return app;
    }
}
using Asp.Versioning;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public BookingsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets a list of all bookings for the authenticated user.
    /// </summary>
    /// <returns>A list of the user's bookings, ordered from most recent to oldest.</returns>
    [HttpGet("my-bookings")]
    [ProducesResponseType(typeof(List<UserBookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "Invalid user identifier in token." });
        }

        var query = new GetUserBookingsQuery(userId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new booking for the authenticated user.
    /// </summary>
    /// <param name="requestDto">The details of the booking to create.</param>
    /// <returns>A confirmation of the created booking.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BookingConfirmationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto requestDto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "Invalid user identifier in token." });
        }

        var command = _mapper.Map<CreateBookingCommand>(requestDto);
        command.UserId = userId;

        var result = await _mediator.Send(command);

        return Created($"/api/v1/bookings/{result.BookingId}", result);
    }
}
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new city.
    /// </summary>
    /// <param name="command">City creation details.</param>
    /// <returns>The created city details.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCity(CreateCityCommand command)
    {
        CityDto cityDto = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCities), new { id = cityDto.Id }, cityDto);
    }

    /// <summary>
    /// Gets a list of all cities.
    /// </summary>
    /// <returns>A list of cities.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<CityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCities()
    {
        List<CityDto> cities = await _mediator.Send(new GetAllCitiesQuery());
        return Ok(cities);
    }

}
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class DealsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateDealDto> _createDealValidator;

    public DealsController(IMediator mediator, IValidator<CreateDealDto> createDealValidator)
    {
        _mediator = mediator;
        _createDealValidator = createDealValidator;
    }

    /// <summary>
    /// Get featured deals for home page display
    /// </summary>
    /// <param name="count">Number of deals to return (default: 10, max: 50)</param>
    /// <returns>List of featured deals</returns>
    [HttpGet("featured")]
    [ResponseCache(Duration = 600)] // Cache for 10 minutes
    [ProducesResponseType(typeof(List<FeaturedDealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFeaturedDeals([FromQuery] int count = 10)
    {
        if (count < 1 || count > 50)
        {
            throw new ArgumentException("Count must be between 1 and 50.", nameof(count));
        }

        var query = new GetFeaturedDealsQuery(count);
        var deals = await _mediator.Send(query);

        return Ok(deals);
    }

    /// <summary>
    /// Get all deals for admin management
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of all deals</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<DealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllDeals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page number must be greater than 0.", nameof(page));
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100.", nameof(pageSize));
        }

        var query = new GetAllDealsQuery(page, pageSize);
        var deals = await _mediator.Send(query);

        return Ok(deals);
    }

    /// <summary>
    /// Get deal by ID
    /// </summary>
    /// <param name="id">Deal ID</param>
    /// <returns>Deal details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DealDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDealById(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid deal ID provided.", nameof(id));
        }

        var query = new GetDealByIdQuery(id);
        var deal = await _mediator.Send(query);

        if (deal == null)
        {
            throw new NotFoundException($"Deal with ID {id} not found.");
        }

        return Ok(deal);
    }

    /// <summary>
    /// Create new deal (Admin only)
    /// </summary>
    /// <param name="dealDto">Deal creation details</param>
    /// <returns>Created deal ID</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDeal([FromBody] CreateDealDto? dealDto)
    {
        // Check for model binding errors first
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => new FluentValidation.Results.ValidationFailure(
                    CleanPropertyName(x.Key),
                    TransformErrorMessage(x.Key, e.ErrorMessage, e.Exception))))
                .ToList();

            throw new ValidationException(modelErrors);
        }

        // Check if model binding resulted in null (happens when JSON structure is completely invalid)
        if (dealDto == null)
        {
            throw new ArgumentException("Invalid request body. Please provide valid deal data.", "dealDto");
        }

        // Validate the DTO manually to get detailed field validation errors
        var validationResult = await _createDealValidator.ValidateAsync(dealDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var command = new CreateDealCommand(dealDto);
        var dealId = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetDealById), new { id = dealId }, dealId);
    }

    private static string CleanPropertyName(string propertyName)
    {
        // Remove JSON path prefix (e.g., "$." from "$.originalPrice")
        if (propertyName.StartsWith("$."))
        {
            return propertyName.Substring(2);
        }
        return propertyName;
    }

    private static string TransformErrorMessage(string propertyName, string? errorMessage, Exception? exception)
    {
        var cleanPropertyName = CleanPropertyName(propertyName);

        // Handle JSON conversion errors
        if (errorMessage?.Contains("could not be converted to") == true)
        {
            if (errorMessage.Contains("System.Decimal"))
            {
                return $"{cleanPropertyName} must be a valid decimal number.";
            }
            if (errorMessage.Contains("System.Int32"))
            {
                return $"{cleanPropertyName} must be a valid integer.";
            }
            if (errorMessage.Contains("System.DateTime"))
            {
                return $"{cleanPropertyName} must be a valid date in ISO 8601 format (e.g., 2025-12-31T23:59:59).";
            }
            if (errorMessage.Contains("System.Guid"))
            {
                return $"{cleanPropertyName} must be a valid GUID.";
            }
            if (errorMessage.Contains("System.Boolean"))
            {
                return $"{cleanPropertyName} must be a valid boolean (true or false).";
            }

            return $"{cleanPropertyName} has an invalid value format.";
        }

        // Handle missing required properties
        if (errorMessage?.Contains("is required") == true)
        {
            return $"{cleanPropertyName} is required.";
        }

        // Return original message if we can't improve it, but clean it up
        if (!string.IsNullOrEmpty(errorMessage))
        {
            // Remove technical details like line numbers and byte positions
            var cleanMessage = errorMessage.Split('|')[0].Trim();
            if (cleanMessage.Contains("Path:"))
            {
                cleanMessage = cleanMessage.Split("Path:")[0].Trim();
            }
            return cleanMessage;
        }

        return exception?.Message ?? $"{cleanPropertyName} has an invalid value.";
    }

    /// <summary>
    /// Update existing deal (Admin only)
    /// </summary>
    /// <param name="id">Deal ID</param>
    /// <param name="dealDto">Deal update details</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeal(Guid id, [FromBody] UpdateDealDto dealDto)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid deal ID provided.", nameof(id));
        }

        var command = new UpdateDealCommand(id, dealDto);
        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Toggle featured status of a deal (Admin only)
    /// </summary>
    /// <param name="id">Deal ID</param>
    /// <returns>No content</returns>
    [HttpPatch("{id}/toggle-featured")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFeatured(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid deal ID provided.", nameof(id));
        }

        var command = new ToggleFeaturedDealCommand(id);
        await _mediator.Send(command);

        return NoContent();
    }
}
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

    /// <summary>
    /// Gets the image gallery for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The ID of the hotel.</param>
    /// <returns>A list of images for the hotel, ordered by sort order.</returns>
    [HttpGet("{hotelId}/gallery")]
    [ProducesResponseType(typeof(List<HotelImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHotelGallery(Guid hotelId)
    {
        var query = new GetHotelGalleryQuery(hotelId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SearchController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SearchController(IMediator mediator, ILogger<SearchController> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _mediator = mediator;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Get search suggestions for autocomplete functionality
    /// </summary>
    /// <param name="q">Search query text</param>
    /// <param name="limit">Maximum number of suggestions to return (default: 10)</param>
    /// <returns>List of search suggestions</returns>
    [HttpGet("suggestions")]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes
    public async Task<ActionResult<List<SearchSuggestionDto>>> GetSearchSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new
            {
                Error = "SearchTextRequired",
                Message = "Search text is required and cannot be empty."
            });
        }

        if (q.Length < 2)
        {
            return BadRequest(new
            {
                Error = "SearchTextTooShort",
                Message = "Search text must be at least 2 characters long."
            });
        }

        if (limit < 1 || limit > 50)
        {
            return BadRequest(new
            {
                Error = "InvalidLimit",
                Message = "Limit must be between 1 and 50."
            });
        }

        _logger.LogDebug("Search suggestions requested for query: {Query} with limit: {Limit}", q, limit);

        var query = new GetSearchSuggestionsQuery(q, limit);
        var suggestions = await _mediator.Send(query);

        _logger.LogDebug("Search suggestions returned {Count} results for query: {Query}", suggestions.Count, q);

        return Ok(new
        {
            Suggestions = suggestions,
            Query = q,
            Count = suggestions.Count
        });
    }

    /// <summary>
    /// Search hotels with filtering and pagination
    /// </summary>
    /// <param name="searchRequest">Search criteria and filters</param>
    /// <returns>Paginated search results</returns>
    [HttpPost("hotels")]
    public async Task<ActionResult<SearchResultDto>> SearchHotels([FromBody] SearchRequestDto searchRequest)
    {
        _logger.LogInformation("Hotel search requested: {SearchText}, CheckIn: {CheckIn}, Adults: {Adults}",
            searchRequest.SearchText, searchRequest.CheckInDate, searchRequest.Adults);

        var stopwatch = Stopwatch.StartNew();
        var query = new SearchHotelsQuery(searchRequest);
        var result = await _mediator.Send(query);
        stopwatch.Stop();

        _logger.LogInformation("Hotel search completed in {ElapsedMs}ms, returned {Count} results",
            stopwatch.ElapsedMilliseconds, result.Hotels.Count);

        return Ok(result);
    }

    /// <summary>
    /// Get popular destinations for trending highlights
    /// </summary>
    /// <param name="count">Number of destinations to return (default: 5)</param>
    /// <returns>List of popular destinations</returns>
    [HttpGet("popular-destinations")]
    [ResponseCache(Duration = 3600)] // Cache for 1 hour
    public async Task<ActionResult<List<CityDto>>> GetPopularDestinations([FromQuery] int count = 5)
    {
        if (count < 1 || count > 20)
        {
            return BadRequest(new
            {
                Error = "InvalidCount",
                Message = "Count must be between 1 and 20."
            });
        }

        var query = new GetPopularDestinationsQuery(count);
        var destinations = await _mediator.Send(query);

        return Ok(new
        {
            Destinations = destinations,
            Count = destinations.Count,
            LastUpdated = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get hotel suggestions autocomplete
    /// </summary>
    /// <param name="q">Search query text</param>
    /// <param name="limit">Maximum number of suggestions to return (default: 10)</param>
    /// <returns>List of hotel suggestions</returns>
    [HttpGet("hotel-suggestions")]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes
    public async Task<ActionResult<List<SearchSuggestionDto>>> GetHotelSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 10)
    {
        var query = new GetSearchSuggestionsQuery(q, limit);
        var suggestions = await _mediator.Send(query);

        // Filter to only hotel suggestions
        var hotelSuggestions = suggestions.Where(s => s.Type == SearchSuggestionType.Hotel).ToList();

        return Ok(new
        {
            Suggestions = hotelSuggestions,
            Query = q,
            Count = hotelSuggestions.Count
        });
    }

    /// <summary>
    /// Get city suggestions autocomplete
    /// </summary>
    /// <param name="q">Search query text</param>
    /// <param name="limit">Maximum number of suggestions to return (default: 10)</param>
    /// <returns>List of city suggestions</returns>
    [HttpGet("city-suggestions")]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes
    public async Task<ActionResult<List<SearchSuggestionDto>>> GetCitySuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 10)
    {
        var query = new GetSearchSuggestionsQuery(q, limit);
        var suggestions = await _mediator.Send(query);

        // Filter to only city suggestions
        var citySuggestions = suggestions.Where(s => s.Type == SearchSuggestionType.City).ToList();

        return Ok(new
        {
            Suggestions = citySuggestions,
            Query = q,
            Count = citySuggestions.Count
        });
    }

    /// <summary>
    /// Get home page data : featured deals, popular destinations, recently visited
    /// </summary>
    /// <returns>Combined home page data with personalization for authenticated users</returns>
    [HttpGet("home-page")]
    [ResponseCache(Duration = 600, VaryByHeader = "Authorization")] // Cache for 10 minutes, vary by authentication
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHomePageData()
    {
        // Check if user is authenticated to provide personalized content
        var userId = GetCurrentUserId();
        var isAuthenticated = userId.HasValue;

        _logger.LogDebug("Home page requested. User authenticated: {IsAuthenticated}, UserId: {UserId}",
            isAuthenticated, userId);

        // Use separate scopes to avoid DbContext concurrency issues
        var featuredDealsTask = Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await mediator.Send(new GetFeaturedDealsQuery(6));
        });

        var popularDestinationsTask = Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await mediator.Send(new GetPopularDestinationsQuery(5));
        });

        // Get recently visited hotels only for authenticated users
        Task<List<RecentlyVisitedHotelDto>> recentlyVisitedTask = Task.FromResult(new List<RecentlyVisitedHotelDto>());

        if (isAuthenticated)
        {
            recentlyVisitedTask = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    return await mediator.Send(new GetRecentlyVisitedHotelsQuery(userId!.Value, 5));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get recently visited hotels for user {UserId}", userId);
                    return new List<RecentlyVisitedHotelDto>();
                }
            });
        }

        // Wait for all tasks to complete
        await Task.WhenAll(featuredDealsTask, popularDestinationsTask, recentlyVisitedTask);

        var recentlyVisited = await recentlyVisitedTask;

        _logger.LogDebug("Home page data retrieved: {FeaturedDealsCount} deals, {PopularDestinationsCount} destinations, {RecentlyVisitedCount} recently visited hotels",
            (await featuredDealsTask).Count, (await popularDestinationsTask).Count, recentlyVisited.Count);

        // Build response with conditional recently visited content
        var response = new
        {
            FeaturedDeals = await featuredDealsTask,
            PopularDestinations = await popularDestinationsTask,
            RecentlyVisited = recentlyVisited, // Empty list for anonymous users
            IsPersonalized = isAuthenticated,
            LastUpdated = DateTime.UtcNow
        };

        return Ok(response);
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
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("TravelBookingPlatform.Modules.Hotels.Api")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Travel Booking Platform")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+93a496f363cd4aacb8bcf1ebace0e342d89deb01")]
[assembly: System.Reflection.AssemblyProductAttribute("TravelBookingPlatform.Modules.Hotels.Api")]
[assembly: System.Reflection.AssemblyTitleAttribute("TravelBookingPlatform.Modules.Hotels.Api")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.

// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("TravelBookingPlatform.Modules.Hotels.Api")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Travel Booking Platform")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+a7c7800de87cfc47bdb963930610f98846c7bdc6")]
[assembly: System.Reflection.AssemblyProductAttribute("TravelBookingPlatform.Modules.Hotels.Api")]
[assembly: System.Reflection.AssemblyTitleAttribute("TravelBookingPlatform.Modules.Hotels.Api")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.

// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.Modules.Hotels.Application.Mapping;
using System.Reflection;

namespace TravelBookingPlatform.Modules.Hotels.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHotelsApplication(this IServiceCollection services)
    {

        // register MediatR Handlers from this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // AutoMapper is registered centrally in the Host project

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
using TravelBookingPlatform.Core.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public class CreateBookingCommand : ICommand<BookingConfirmationDto>
{
    public Guid UserId { get; set; }
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public int NumberOfRooms { get; set; } = 1;
}
using TravelBookingPlatform.Core.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public class CreateCityCommand : ICommand<CityDto>
{
    public string Name { get; set; }
    public string Country { get; set; }
    public string PostCode { get; set; }
}
using TravelBookingPlatform.Core.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public record CreateDealCommand(CreateDealDto DealData) : ICommand<Guid>;
using TravelBookingPlatform.Core.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public record ToggleFeaturedDealCommand(Guid Id) : ICommand;
using TravelBookingPlatform.Core.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public record UpdateDealCommand(Guid Id, UpdateDealDto DealData) : ICommand;
using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingConfirmationDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IHotelRepository hotelRepository,
        IRoomTypeRepository roomTypeRepository,
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _hotelRepository = hotelRepository;
        _roomTypeRepository = roomTypeRepository;
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingConfirmationDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate existence of related entities
        var hotel = await _hotelRepository.GetByIdAsync(request.HotelId)
            ?? throw new NotFoundException(nameof(Hotel), request.HotelId);

        var roomType = await _roomTypeRepository.GetByIdAsync(request.RoomTypeId)
            ?? throw new NotFoundException(nameof(RoomType), request.RoomTypeId);

        // 2. Validate business rules
        if (!roomType.CanAccommodate(request.NumberOfAdults, request.NumberOfChildren))
        {
            throw new BusinessValidationException("The selected room type cannot accommodate the specified number of guests.", "NumberOfAdults");
        }

        // 3. Check for availability (This is the critical race-condition check)
        var availableRooms = await _roomRepository.GetAvailableRoomsByTypeForPeriodAsync(
            request.HotelId, request.RoomTypeId, request.CheckInDate, request.CheckOutDate);

        if (availableRooms.Count < request.NumberOfRooms)
        {
            throw new BusinessValidationException("The selected room type is no longer available for the chosen dates. Please try again.", "RoomTypeId");
        }

        // 4. Create Booking(s) in a transaction
        Booking newBooking;
        try
        {
            var roomToBook = availableRooms.First(); // For this simple case, we book the first available room.
                                                     // For booking multiple rooms, we would iterate 'request.NumberOfRooms' times.

            newBooking = new Booking(
                request.CheckInDate,
                request.CheckOutDate,
                roomToBook.Id,
                request.UserId);

            await _bookingRepository.AddAsync(newBooking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // The Unit of Work will handle the rollback.
            throw new Exception("Failed to save the booking. Please try again.", ex);
        }

        // 5. Return confirmation DTO
        var totalNights = newBooking.GetNumberOfNights();
        return new BookingConfirmationDto
        {
            BookingId = newBooking.Id,
            ConfirmationNumber = $"BKG-{newBooking.Id.ToString().ToUpper().Split('-').First()}",
            HotelName = hotel.Name,
            RoomTypeName = roomType.Name,
            CheckInDate = newBooking.CheckInDate,
            CheckOutDate = newBooking.CheckOutDate,
            TotalNights = totalNights,
            TotalPrice = totalNights * roomType.PricePerNight
        };
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class CreateCityCommandHandler: IRequestHandler<CreateCityCommand, CityDto>
{
    private readonly ICityRepository _cityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCityCommandHandler(ICityRepository cityRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _cityRepository = cityRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CityDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        City city = new City(request.Name, request.Country, request.PostCode);
            
        await _cityRepository.AddAsync(city);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
            
        return _mapper.Map<CityDto>(city);
    }
}
using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class CreateDealCommandHandler : IRequestHandler<CreateDealCommand, Guid>
{
    private readonly IDealRepository _dealRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDealCommandHandler(
        IDealRepository dealRepository,
        IHotelRepository hotelRepository,
        IRoomTypeRepository roomTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _hotelRepository = hotelRepository;
        _roomTypeRepository = roomTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
    {
        var dealData = request.DealData;

        // Validate that the hotel exists
        var hotel = await _hotelRepository.GetByIdAsync(dealData.HotelId);
        if (hotel == null)
        {
            throw new ForeignKeyViolationException("Deal", "Hotel", dealData.HotelId);
        }

        // Validate that the room type exists
        var roomType = await _roomTypeRepository.GetByIdAsync(dealData.RoomTypeId);
        if (roomType == null)
        {
            throw new ForeignKeyViolationException("Deal", "RoomType", dealData.RoomTypeId);
        }

        // Check for overlapping deals
        var overlappingDeal = await _dealRepository.GetOverlappingDealAsync(
            dealData.HotelId,
            dealData.RoomTypeId,
            dealData.ValidFrom,
            dealData.ValidTo);

        if (overlappingDeal != null)
        {
            throw new BusinessValidationException(
                $"An active deal already exists for this hotel and room type during the specified period. Existing deal: '{overlappingDeal.Title}' ({overlappingDeal.ValidFrom:yyyy-MM-dd} to {overlappingDeal.ValidTo:yyyy-MM-dd})",
                "DateRange");
        }

        // Check for duplicate title
        var existingTitleDeal = await _dealRepository.GetByHotelAndTitleAsync(
            dealData.HotelId,
            dealData.Title);

        if (existingTitleDeal != null)
        {
            throw new BusinessValidationException(
                $"A deal with title '{dealData.Title}' already exists for this hotel.",
                "Title");
        }

        try
        {
            var deal = new Deal(
                dealData.Title,
                dealData.Description,
                dealData.HotelId,
                dealData.OriginalPrice,
                dealData.DiscountedPrice,
                dealData.ValidFrom,
                dealData.ValidTo,
                dealData.IsFeatured,
                dealData.RoomTypeId,
                dealData.MaxBookings,
                dealData.ImageURL
            );

            await _dealRepository.AddAsync(deal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return deal.Id;
        }
        catch (ArgumentException ex)
        {
            throw new BusinessValidationException(ex.Message, ex.ParamName);
        }
    }
}
using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class ToggleFeaturedDealCommandHandler : IRequestHandler<ToggleFeaturedDealCommand>
{
    private readonly IDealRepository _dealRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleFeaturedDealCommandHandler(IDealRepository dealRepository, IUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ToggleFeaturedDealCommand request, CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdAsync(request.Id);
        if (deal == null)
            throw new NotFoundException("Deal", request.Id);

        deal.ToggleFeatured();
        _dealRepository.Update(deal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class UpdateDealCommandHandler : IRequestHandler<UpdateDealCommand>
{
    private readonly IDealRepository _dealRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDealCommandHandler(IDealRepository dealRepository, IRoomTypeRepository roomTypeRepository, IUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _roomTypeRepository = roomTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDealCommand request, CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdAsync(request.Id);
        if (deal == null)
            throw new NotFoundException("Deal", request.Id);

        var dealData = request.DealData;

        // Validate that the room type exists
        var roomType = await _roomTypeRepository.GetByIdAsync(dealData.RoomTypeId);
        if (roomType == null)
        {
            throw new ForeignKeyViolationException("Deal", "RoomType", dealData.RoomTypeId);
        }

        try
        {
            deal.Update(
                dealData.Title,
                dealData.Description,
                dealData.RoomTypeId,
                dealData.OriginalPrice,
                dealData.DiscountedPrice,
                dealData.ValidFrom,
                dealData.ValidTo,
                dealData.MaxBookings,
                dealData.ImageURL
            );

            if (dealData.IsFeatured != deal.IsFeatured)
            {
                deal.ToggleFeatured();
            }

            _dealRepository.Update(deal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ArgumentException ex)
        {
            throw new BusinessValidationException(ex.Message, ex.ParamName);
        }
    }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class AvailableRoomTypeDto
{
    public Guid RoomTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }
    public string? ImageUrl { get; set; }
    public int NumberOfAvailableRooms { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class BookingConfirmationDto
{
    public Guid BookingId { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int TotalNights { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Confirmed";
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class CityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string PostCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class CreateBookingRequestDto
{
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public int NumberOfRooms { get; set; } = 1;
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class CreateCityDto
{
    public string Name { get; set; }
    public string Country { get; set; }
    public string PostCode { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class CreateDealDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsFeatured { get; set; }
    public int MaxBookings { get; set; } = 100;
    public string? ImageURL { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class DealDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid HotelId { get; set; }
    public Guid? RoomTypeId { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public int MaxBookings { get; set; }
    public int CurrentBookings { get; set; }
    public string? ImageURL { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public HotelSummaryDto Hotel { get; set; } = new();
    public RoomTypeSummaryDto? RoomType { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class FeaturedDealDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime ValidTo { get; set; }
    public string? ImageURL { get; set; }
    public HotelSummaryDto Hotel { get; set; } = new();
    public RoomTypeSummaryDto? RoomType { get; set; }
}

public class HotelSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ImageURL { get; set; }
}

public class RoomTypeSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class HotelDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public List<HotelImageDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // City information
    public CityDto City { get; set; } = null!;

    // Room information
    public List<RoomDetailDto> Rooms { get; set; } = new();

    // Summary statistics
    public int TotalRooms { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string> AvailableRoomTypes { get; set; } = new();
}

public class RoomDetailDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Room type information
    public RoomTypeDetailDto RoomType { get; set; } = null!;
}

public class RoomTypeDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class HotelImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
    public bool IsCoverImage { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class RecentlyVisitedHotelDto
{
    public Guid HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime LastVisitedDate { get; set; }
    public int VisitCount { get; set; }

    // City information
    public Guid CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Price range information (from room types)
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class SearchRequestDto
{
    public string? SearchText { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; } = 1;
    public int Adults { get; set; } = 2;
    public int Children { get; set; } = 0;
    public decimal? MinRating { get; set; }
    public decimal? MaxRating { get; set; }
    public Guid? CityId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "Relevance";
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class SearchResultDto
{
    public List<HotelSearchResultDto> Hotels { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public TimeSpan QueryTime { get; set; }
}

public class HotelSearchResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal? PricePerNight { get; set; }
    public List<HotelImageDto> Images { get; set; } = new();
    public int AvailableRooms { get; set; }
    public bool IsAvailable { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class SearchSuggestionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SearchSuggestionType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public enum SearchSuggestionType
{
    Hotel,
    City
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class UpdateDealDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid RoomTypeId { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsFeatured { get; set; }
    public int MaxBookings { get; set; }
    public string? ImageURL { get; set; }
}
namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class UserBookingDto
{
    public Guid BookingId { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty; // e.g., "Upcoming", "In Progress", "Completed"

    // Hotel Details
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string? HotelImageUrl { get; set; }
    public string CityName { get; set; } = string.Empty;

    // Room & Stay Details
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int TotalNights { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookedAt { get; set; }
}
using AutoMapper;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Application.Mapping;

public class HotelsMappingProfile : Profile
{
    public HotelsMappingProfile()
    {
        CreateMap<City, CityDto>();

        // Deal mappings
        CreateMap<Deal, FeaturedDealDto>()
            .ForMember(dest => dest.Hotel, opt => opt.MapFrom(src => new HotelSummaryDto
            {
                Id = src.Hotel.Id,
                Name = src.Hotel.Name,
                Rating = src.Hotel.Rating,
                City = src.Hotel.City.Name,
                Country = src.Hotel.City.Country,
            }))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType != null ? new RoomTypeSummaryDto
            {
                Id = src.RoomType.Id,
                Name = src.RoomType.Name,
                MaxAdults = src.RoomType.MaxAdults,
                MaxChildren = src.RoomType.MaxChildren
            } : null));

        CreateMap<Deal, DealDto>()
            .ForMember(dest => dest.Hotel, opt => opt.MapFrom(src => new HotelSummaryDto
            {
                Id = src.Hotel.Id,
                Name = src.Hotel.Name,
                Rating = src.Hotel.Rating,
                City = src.Hotel.City.Name,
                Country = src.Hotel.City.Country,
            }))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType != null ? new RoomTypeSummaryDto
            {
                Id = src.RoomType.Id,
                Name = src.RoomType.Name,
                MaxAdults = src.RoomType.MaxAdults,
                MaxChildren = src.RoomType.MaxChildren
            } : null));

        // Hotel detail mappings
        CreateMap<Hotel, HotelDetailDto>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.SortOrder).ToList()));

        CreateMap<Room, RoomDetailDto>();

        CreateMap<RoomType, RoomTypeDetailDto>();

        CreateMap<CreateBookingRequestDto, CreateBookingCommand>();

        CreateMap<Booking, UserBookingDto>()
    .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.ConfirmationNumber, opt => opt.MapFrom(src => $"BKG-{src.Id.ToString().Substring(0, 8).ToUpper()}"))
    .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.Room.Hotel.Id))
    .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Room.Hotel.Name))
    .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.Room.Hotel.City.Name))
    .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.Room.RoomType.Name))
    .ForMember(dest => dest.TotalNights, opt => opt.MapFrom(src => src.GetNumberOfNights()))
    .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.GetNumberOfNights() * src.Room.RoomType.PricePerNight))
    .ForMember(dest => dest.BookedAt, opt => opt.MapFrom(src => src.CreatedAt))
    .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src =>
        src.CheckOutDate.Date < DateTime.Today ? "Completed"
        : src.CheckInDate.Date > DateTime.Today ? "Upcoming"
        : "In Progress"));

        CreateMap<HotelImage, HotelImageDto>();
    }


}
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("TravelBookingPlatform.Modules.Hotels.Application")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Travel Booking Platform")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+93a496f363cd4aacb8bcf1ebace0e342d89deb01")]
[assembly: System.Reflection.AssemblyProductAttribute("TravelBookingPlatform.Modules.Hotels.Application")]
[assembly: System.Reflection.AssemblyTitleAttribute("TravelBookingPlatform.Modules.Hotels.Application")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.

// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("TravelBookingPlatform.Modules.Hotels.Application")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Travel Booking Platform")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+a7c7800de87cfc47bdb963930610f98846c7bdc6")]
[assembly: System.Reflection.AssemblyProductAttribute("TravelBookingPlatform.Modules.Hotels.Application")]
[assembly: System.Reflection.AssemblyTitleAttribute("TravelBookingPlatform.Modules.Hotels.Application")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.

// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Core.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public class GetAllCitiesQuery: IQuery<List<CityDto>>
{
    // no specific implementation needed
}
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Core.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetAllDealsQuery(int Page = 1, int PageSize = 20) : IQuery<List<DealDto>>;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Core.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetDealByIdQuery(Guid Id) : IQuery<DealDto?>;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Core.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetFeaturedDealsQuery(int Count = 10) : IQuery<List<FeaturedDealDto>>;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Core.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetHotelByIdQuery(Guid Id) : IQuery<HotelDetailDto?>;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetHotelGalleryQuery(Guid HotelId) : IRequest<List<HotelImageDto>>;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetHotelRoomAvailabilityQuery(
    Guid HotelId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfAdults,
    int NumberOfChildren) : IRequest<List<AvailableRoomTypeDto>>;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetPopularDestinationsQuery(int Count = 5) : IRequest<List<CityDto>>;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetRecentlyVisitedHotelsQuery(Guid UserId, int Limit = 10) : IRequest<List<RecentlyVisitedHotelDto>>;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetSearchSuggestionsQuery(
    string SearchText,
    int MaxResults = 10) : IRequest<List<SearchSuggestionDto>>;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetUserBookingsQuery(Guid UserId) : IRequest<List<UserBookingDto>>;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record SearchHotelsQuery(SearchRequestDto SearchRequest) : IRequest<SearchResultDto>;
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetAllCitiesQueryHandler : IRequestHandler<GetAllCitiesQuery, List<CityDto>>
{
    private readonly ICityRepository _cityRepository;
    private readonly IMapper _mapper;

    public GetAllCitiesQueryHandler(ICityRepository cityRepository, IMapper mapper)
    {
        _cityRepository = cityRepository;
        _mapper = mapper;
    }

    public async Task<List<CityDto>> Handle(GetAllCitiesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<City> cities = await _cityRepository.GetAllAsync();
        return _mapper.Map<List<CityDto>>(cities);
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetAllDealsQueryHandler : IRequestHandler<GetAllDealsQuery, List<DealDto>>
{
    private readonly IDealRepository _dealRepository;
    private readonly IMapper _mapper;

    public GetAllDealsQueryHandler(IDealRepository dealRepository, IMapper mapper)
    {
        _dealRepository = dealRepository;
        _mapper = mapper;
    }

    public async Task<List<DealDto>> Handle(GetAllDealsQuery request, CancellationToken cancellationToken)
    {
        var deals = await _dealRepository.GetActiveDealsAsync();
        return _mapper.Map<List<DealDto>>(deals);
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetDealByIdQueryHandler : IRequestHandler<GetDealByIdQuery, DealDto?>
{
    private readonly IDealRepository _dealRepository;
    private readonly IMapper _mapper;

    public GetDealByIdQueryHandler(IDealRepository dealRepository, IMapper mapper)
    {
        _dealRepository = dealRepository;
        _mapper = mapper;
    }

    public async Task<DealDto?> Handle(GetDealByIdQuery request, CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetDealWithDetailsAsync(request.Id);
        return deal == null ? null : _mapper.Map<DealDto>(deal);
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetFeaturedDealsQueryHandler : IRequestHandler<GetFeaturedDealsQuery, List<FeaturedDealDto>>
{
    private readonly IDealRepository _dealRepository;
    private readonly IMapper _mapper;

    public GetFeaturedDealsQueryHandler(IDealRepository dealRepository, IMapper mapper)
    {
        _dealRepository = dealRepository;
        _mapper = mapper;
    }

    public async Task<List<FeaturedDealDto>> Handle(GetFeaturedDealsQuery request, CancellationToken cancellationToken)
    {
        var deals = await _dealRepository.GetFeaturedDealsAsync(request.Count);
        return _mapper.Map<List<FeaturedDealDto>>(deals);
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDetailDto?>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public GetHotelByIdQueryHandler(IHotelRepository hotelRepository, IMapper mapper)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<HotelDetailDto?> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetHotelWithDetailsAsync(request.Id);

        if (hotel == null)
            return null;

        var hotelDetailDto = _mapper.Map<HotelDetailDto>(hotel);

        // Calculate summary statistics
        hotelDetailDto.TotalRooms = hotel.Rooms.Count;
        hotelDetailDto.MinPrice = hotel.Rooms.Any() ? hotel.Rooms.Min(r => r.RoomType.PricePerNight) : null;
        hotelDetailDto.MaxPrice = hotel.Rooms.Any() ? hotel.Rooms.Max(r => r.RoomType.PricePerNight) : null;
        hotelDetailDto.AvailableRoomTypes = hotel.Rooms
            .Select(r => r.RoomType.Name)
            .Distinct()
            .ToList();

        return hotelDetailDto;
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetHotelGalleryQueryHandler : IRequestHandler<GetHotelGalleryQuery, List<HotelImageDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public GetHotelGalleryQueryHandler(IHotelRepository hotelRepository, IMapper mapper)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<List<HotelImageDto>> Handle(GetHotelGalleryQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetHotelWithImagesAsync(request.HotelId);

        if (hotel is null)
        {
            throw new NotFoundException(nameof(Hotel), request.HotelId);
        }

        var images = hotel.Images.OrderBy(i => i.SortOrder).ToList();
        return _mapper.Map<List<HotelImageDto>>(images);
    }
}
using MediatR;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetHotelRoomAvailabilityQueryHandler : IRequestHandler<GetHotelRoomAvailabilityQuery, List<AvailableRoomTypeDto>>
{
    private readonly IHotelRepository _hotelRepository;

    public GetHotelRoomAvailabilityQueryHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<List<AvailableRoomTypeDto>> Handle(GetHotelRoomAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetHotelWithRoomsAndBookingsAsync(request.HotelId);

        if (hotel is null)
        {
            throw new NotFoundException(nameof(Hotel), request.HotelId);
        }

        var availableRoomTypes = hotel.Rooms
            .Where(room => room.RoomType.CanAccommodate(request.NumberOfAdults, request.NumberOfChildren))
            .Where(room => room.IsAvailableForPeriod(request.CheckInDate, request.CheckOutDate))
            .GroupBy(room => room.RoomType)
            .Select(group => new AvailableRoomTypeDto
            {
                RoomTypeId = group.Key.Id,
                Name = group.Key.Name,
                Description = group.Key.Description,
                PricePerNight = group.Key.PricePerNight,
                MaxAdults = group.Key.MaxAdults,
                MaxChildren = group.Key.MaxChildren,
                ImageUrl = group.Key.ImageUrl,
                NumberOfAvailableRooms = group.Count()
            })
            .OrderBy(dto => dto.PricePerNight)
            .ToList();

        return availableRoomTypes;
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetPopularDestinationsQueryHandler : IRequestHandler<GetPopularDestinationsQuery, List<CityDto>>
{
    private readonly ICityRepository _cityRepository;
    private readonly IMapper _mapper;

    public GetPopularDestinationsQueryHandler(ICityRepository cityRepository, IMapper mapper)
    {
        _cityRepository = cityRepository;
        _mapper = mapper;
    }

    public async Task<List<CityDto>> Handle(GetPopularDestinationsQuery request, CancellationToken cancellationToken)
    {
        var popularCities = await _cityRepository.GetPopularDestinationsAsync(request.Count);
        return _mapper.Map<List<CityDto>>(popularCities);
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Identity.Application.Services;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetRecentlyVisitedHotelsQueryHandler : IRequestHandler<GetRecentlyVisitedHotelsQuery, List<RecentlyVisitedHotelDto>>
{
    private readonly IActivityTrackingService _activityTrackingService;
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public GetRecentlyVisitedHotelsQueryHandler(
        IActivityTrackingService activityTrackingService,
        IHotelRepository hotelRepository,
        IMapper mapper)
    {
        _activityTrackingService = activityTrackingService;
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<List<RecentlyVisitedHotelDto>> Handle(GetRecentlyVisitedHotelsQuery request, CancellationToken cancellationToken)
    {
        // Get recent hotel view activities for the user
        var recentActivities = await _activityTrackingService.GetRecentActivitiesAsync(
            request.UserId,
            ActivityType.HotelView,
            request.Limit * 2); // Get more activities to account for potential duplicates

        if (!recentActivities.Any())
        {
            return new List<RecentlyVisitedHotelDto>();
        }

        // Group activities by hotel to get visit counts and last visited dates
        var hotelActivities = recentActivities
            .Where(a => a.TargetId.HasValue && a.TargetType == "Hotel")
            .GroupBy(a => a.TargetId!.Value)
            .Select(g => new
            {
                HotelId = g.Key,
                LastVisitedDate = g.Max(a => a.ActivityDate),
                VisitCount = g.Count()
            })
            .OrderByDescending(x => x.LastVisitedDate)
            .Take(request.Limit)
            .ToList();

        if (!hotelActivities.Any())
        {
            return new List<RecentlyVisitedHotelDto>();
        }

        // Get hotel details for the visited hotels
        var hotelIds = hotelActivities.Select(ha => ha.HotelId).ToList();
        var hotels = await _hotelRepository.GetHotelsWithDetailsAsync(hotelIds);

        // Combine activity data with hotel data
        var result = new List<RecentlyVisitedHotelDto>();

        foreach (var hotelActivity in hotelActivities)
        {
            var hotel = hotels.FirstOrDefault(h => h.Id == hotelActivity.HotelId);
            if (hotel == null) continue; // Skip if hotel not found (might be deleted)

            var dto = new RecentlyVisitedHotelDto
            {
                HotelId = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Rating = hotel.Rating,
                LastVisitedDate = hotelActivity.LastVisitedDate,
                VisitCount = hotelActivity.VisitCount,
                CityId = hotel.CityId,
                CityName = hotel.City?.Name ?? string.Empty,
                Country = hotel.City?.Country ?? string.Empty,
                MinPrice = hotel.Rooms?.Where(r => r.RoomType != null)
                                    .Min(r => r.RoomType!.PricePerNight),
                MaxPrice = hotel.Rooms?.Where(r => r.RoomType != null)
                                    .Max(r => r.RoomType!.PricePerNight)
            };

            result.Add(dto);
        }

        return result;
    }
}
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetSearchSuggestionsQueryHandler : IRequestHandler<GetSearchSuggestionsQuery, List<SearchSuggestionDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ICityRepository _cityRepository;

    public GetSearchSuggestionsQueryHandler(
        IHotelRepository hotelRepository,
        ICityRepository cityRepository)
    {
        _hotelRepository = hotelRepository;
        _cityRepository = cityRepository;
    }

    public async Task<List<SearchSuggestionDto>> Handle(GetSearchSuggestionsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchText) || request.SearchText.Length < 2)
            return new List<SearchSuggestionDto>();

        var suggestions = new List<SearchSuggestionDto>();

        // Get hotel suggestions
        var hotels = await _hotelRepository.GetHotelSuggestionsAsync(request.SearchText, request.MaxResults / 2);
        foreach (var hotel in hotels)
        {
            suggestions.Add(new SearchSuggestionDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Type = SearchSuggestionType.Hotel,
                Location = $"{hotel.City.Name}, {hotel.City.Country}",
            });
        }

        // Get city suggestions
        var cities = await _cityRepository.GetCitySuggestionsAsync(request.SearchText, request.MaxResults / 2);
        foreach (var city in cities)
        {
            suggestions.Add(new SearchSuggestionDto
            {
                Id = city.Id,
                Name = city.Name,
                Type = SearchSuggestionType.City,
                Location = $"{city.Name}, {city.Country}",
                ImageUrl = null
            });
        }

        return suggestions.Take(request.MaxResults).ToList();
    }
}
using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, List<UserBookingDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public GetUserBookingsQueryHandler(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<List<UserBookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(request.UserId);

        var bookingDtos = _mapper.Map<List<UserBookingDto>>(bookings);

        return bookingDtos;
    }
}
using AutoMapper;
using MediatR;
using System.Diagnostics;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.ValueObjects;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class SearchHotelsQueryHandler : IRequestHandler<SearchHotelsQuery, SearchResultDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public SearchHotelsQueryHandler(IHotelRepository hotelRepository, IMapper mapper)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<SearchResultDto> Handle(SearchHotelsQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var searchCriteria = SearchCriteria.Create(
            searchText: request.SearchRequest.SearchText,
            checkInDate: request.SearchRequest.CheckInDate,
            checkOutDate: request.SearchRequest.CheckOutDate,
            numberOfRooms: request.SearchRequest.NumberOfRooms,
            adults: request.SearchRequest.Adults,
            children: request.SearchRequest.Children,
            minRating: request.SearchRequest.MinRating,
            maxRating: request.SearchRequest.MaxRating,
            cityId: request.SearchRequest.CityId,
            pageNumber: request.SearchRequest.PageNumber,
            pageSize: request.SearchRequest.PageSize,
            sortBy: request.SearchRequest.SortBy);

        var (hotels, totalCount) = await _hotelRepository.SearchHotelsWithPaginationAsync(searchCriteria);

        var hotelDtos = new List<HotelSearchResultDto>();
        foreach (var hotel in hotels)
        {
            var availableRooms = 0;
            var isAvailable = true;
            decimal? minPrice = null;

            // Calculate available rooms and pricing if date range is provided
            if (searchCriteria.HasDateRange)
            {
                availableRooms = hotel.Rooms.Count(room =>
                    room.IsAvailableForPeriod(searchCriteria.CheckInDate!.Value, searchCriteria.CheckOutDate!.Value) &&
                    room.RoomType.CanAccommodate(searchCriteria.Adults, searchCriteria.Children));

                isAvailable = availableRooms >= searchCriteria.NumberOfRooms;

                if (availableRooms > 0)
                {
                    minPrice = hotel.Rooms
                        .Where(room => room.IsAvailableForPeriod(searchCriteria.CheckInDate!.Value, searchCriteria.CheckOutDate!.Value) &&
                                     room.RoomType.CanAccommodate(searchCriteria.Adults, searchCriteria.Children))
                        .Min(room => room.RoomType.PricePerNight);
                }
            }
            else
            {
                availableRooms = hotel.Rooms.Count(room =>
                    room.RoomType.CanAccommodate(searchCriteria.Adults, searchCriteria.Children));

                isAvailable = availableRooms >= searchCriteria.NumberOfRooms;

                if (availableRooms > 0)
                {
                    minPrice = hotel.Rooms
                        .Where(room => room.RoomType.CanAccommodate(searchCriteria.Adults, searchCriteria.Children))
                        .Min(room => room.RoomType.PricePerNight);
                }
            }

            hotelDtos.Add(new HotelSearchResultDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Rating = hotel.Rating,
                City = hotel.City.Name,
                Country = hotel.City.Country,
                PricePerNight = minPrice,
                AvailableRooms = availableRooms,
                IsAvailable = isAvailable,
                Images = _mapper.Map<List<HotelImageDto>>(hotel.Images.OrderBy(i => i.SortOrder).ToList())
            });
        }

        stopwatch.Stop();

        return new SearchResultDto
        {
            Hotels = hotelDtos,
            TotalCount = totalCount,
            PageNumber = searchCriteria.PageNumber,
            PageSize = searchCriteria.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / searchCriteria.PageSize),
            QueryTime = stopwatch.Elapsed
        };
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.HotelId).NotEmpty();
        RuleFor(x => x.RoomTypeId).NotEmpty();

        RuleFor(x => x.CheckInDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .NotEmpty()
            .GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after check-in.");

        RuleFor(x => x.NumberOfAdults)
            .InclusiveBetween(1, 10).WithMessage("Number of adults must be between 1 and 10.");

        RuleFor(x => x.NumberOfChildren)
            .InclusiveBetween(0, 10).WithMessage("Number of children must be between 0 and 10.");

        RuleFor(x => x.NumberOfRooms)
            .InclusiveBetween(1, 5).WithMessage("You can book between 1 and 5 rooms at a time.");
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using System.Linq;
using System.Text.RegularExpressions;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
{
    private readonly ICityRepository _cityRepository;

    public CreateCityCommandValidator(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("City Name is required")
            .MinimumLength(2).WithMessage("City Name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("City Name must not exceed 100 characters")
            .Matches(@"^[\p{L}\s\-'\.]+$").WithMessage("City Name can only contain letters, spaces, hyphens, apostrophes, and periods")
            .MustAsync(async (name, cancellation) => !await _cityRepository.NameExistsAsync(name))
            .WithMessage("A city with this name already exists");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country Name is required")
            .MinimumLength(2).WithMessage("Country Name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("Country Name must not exceed 100 characters")
            .Matches(@"^[\p{L}\s\-'\.]+$").WithMessage("Country Name can only contain letters, spaces, hyphens, apostrophes, and periods (no numbers allowed)");

        RuleFor(x => x.PostCode)
            .NotEmpty().WithMessage("PostCode is required")
            .MinimumLength(3).WithMessage("PostCode must be at least 3 characters long")
            .MaximumLength(20).WithMessage("PostCode must not exceed 20 characters")
            .Matches(@"^[A-Za-z0-9\s\-]+$").WithMessage("PostCode can only contain letters, numbers, spaces, and hyphens")
            .Must(BeValidPostCodeFormat).WithMessage("PostCode must follow a valid format (e.g., 12345, SW1A 1AA, K1A-0A6)")
            .MustAsync(async (postCode, cancellation) => !await _cityRepository.PostCodeExistsAsync(postCode))
            .WithMessage("A city with this post code already exists");
    }

    private bool BeValidPostCodeFormat(string postCode)
    {
        if (string.IsNullOrEmpty(postCode))
            return false;

        // Common postcode patterns:
        // US: 5 digits (12345) or 9 digits (12345-6789)
        // UK: Various formats like SW1A 1AA, M1 1AA, B33 8TH
        // Canada: A1A 1A1 or A1A-1A1
        // General: Allow alphanumeric with spaces and hyphens

        var patterns = new[]
        {
            @"^\d{5}$",                           // US 5-digit
            @"^\d{5}-\d{4}$",                    // US 9-digit
            @"^[A-Z]{1,2}\d{1,2}[A-Z]?\s?\d[A-Z]{2}$", // UK format
            @"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$",      // Canada
            @"^[A-Z]\d[A-Z]-\d[A-Z]\d$",        // Canada with hyphen
            @"^\d{4}\s?[A-Z]{2}$",              // Netherlands
            @"^\d{5}$",                         // Germany
            @"^\d{3}-\d{4}$",                   // Some other formats
            @"^[A-Za-z0-9]+$"                   // Custom pattern: letters and digits only, e.g. P405, 405A, C405A
        };
        
        return patterns.Any(pattern => Regex.IsMatch(postCode.ToUpper(), pattern));
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateDealCommandValidator : AbstractValidator<CreateDealCommand>
{
    public CreateDealCommandValidator()
    {
        // Only validate that DealData is not null - field validation is handled by CreateDealDtoValidator
        RuleFor(x => x.DealData)
            .NotNull().WithMessage("Deal data is required.");
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class CreateDealDtoValidator : AbstractValidator<CreateDealDto>
{
    public CreateDealDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Deal title is required.")
            .MaximumLength(200).WithMessage("Deal title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Deal description is required.")
            .MaximumLength(2000).WithMessage("Deal description must not exceed 2000 characters.");

        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.RoomTypeId)
            .NotEmpty().WithMessage("Room type ID is required.");

        RuleFor(x => x.OriginalPrice)
            .GreaterThan(0).WithMessage("Original price must be greater than zero.")
            .LessThanOrEqualTo(100000).WithMessage("Original price must not exceed $100,000.");

        RuleFor(x => x.DiscountedPrice)
            .GreaterThan(0).WithMessage("Discounted price must be greater than zero.")
            .LessThanOrEqualTo(100000).WithMessage("Discounted price must not exceed $100,000.");

        RuleFor(x => x)
            .Must(x => x.DiscountedPrice < x.OriginalPrice)
            .WithMessage("Discounted price must be less than original price.")
            .WithName("DiscountedPrice");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("Valid from date is required.")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Valid from date cannot be in the past.");

        RuleFor(x => x.ValidTo)
            .NotEmpty().WithMessage("Valid to date is required.");

        RuleFor(x => x)
            .Must(x => x.ValidTo > x.ValidFrom)
            .WithMessage("Valid to date must be after valid from date.")
            .WithName("ValidTo");

        RuleFor(x => x.MaxBookings)
            .GreaterThan(0).WithMessage("Max bookings must be greater than zero.")
            .LessThanOrEqualTo(10000).WithMessage("Max bookings must not exceed 10,000.");

        RuleFor(x => x.ImageURL)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageURL));
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class GetHotelRoomAvailabilityQueryValidator : AbstractValidator<GetHotelRoomAvailabilityQuery>
{
    public GetHotelRoomAvailabilityQueryValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.CheckInDate)
            .NotEmpty().WithMessage("Check-in date is required.")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .NotEmpty().WithMessage("Check-out date is required.")
            .GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after the check-in date.");

        RuleFor(x => x.NumberOfAdults)
            .GreaterThan(0).WithMessage("Number of adults must be at least 1.");

        RuleFor(x => x.NumberOfChildren)
            .GreaterThanOrEqualTo(0).WithMessage("Number of children cannot be negative.");
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class GetSearchSuggestionsQueryValidator : AbstractValidator<GetSearchSuggestionsQuery>
{
    public GetSearchSuggestionsQueryValidator()
    {
        RuleFor(x => x.SearchText)
            .NotEmpty().WithMessage("Search text is required and cannot be empty.")
            .MinimumLength(2).WithMessage("Search text must be at least 2 characters long.");

        RuleFor(x => x.MaxResults)
            .InclusiveBetween(1, 50).WithMessage("Limit must be between 1 and 50.");
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class SearchRequestDtoValidator : AbstractValidator<SearchRequestDto>
{
    public SearchRequestDtoValidator()
    {
        RuleFor(x => x.SearchText)
            .MinimumLength(2)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText))
            .WithMessage("Search text must be at least 2 characters long.")
            .MaximumLength(500)
            .WithMessage("Search text cannot exceed 500 characters.");

        RuleFor(x => x.CheckInDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .When(x => x.CheckInDate.HasValue)
            .WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .GreaterThan(x => x.CheckInDate)
            .When(x => x.CheckInDate.HasValue && x.CheckOutDate.HasValue)
            .WithMessage("Check-out date must be after check-in date.")
            .LessThanOrEqualTo(DateTime.Today.AddYears(2))
            .When(x => x.CheckOutDate.HasValue)
            .WithMessage("Check-out date cannot be more than 2 years in the future.");

        RuleFor(x => x.NumberOfRooms)
            .GreaterThan(0)
            .LessThanOrEqualTo(10)
            .WithMessage("Number of rooms must be between 1 and 10.");

        RuleFor(x => x.Adults)
            .GreaterThan(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Number of adults must be between 1 and 20.");

        RuleFor(x => x.Children)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Number of children must be between 0 and 20.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5)
            .When(x => x.MinRating.HasValue)
            .WithMessage("Minimum rating must be between 0 and 5.");

        RuleFor(x => x.MaxRating)
            .InclusiveBetween(0, 5)
            .When(x => x.MaxRating.HasValue)
            .WithMessage("Maximum rating must be between 0 and 5.");

        RuleFor(x => x.MaxRating)
            .GreaterThanOrEqualTo(x => x.MinRating)
            .When(x => x.MinRating.HasValue && x.MaxRating.HasValue)
            .WithMessage("Maximum rating must be greater than or equal to minimum rating.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "Relevance", "Price", "Rating", "Name" }.Contains(sortBy))
            .WithMessage("Sort by must be one of: Relevance, Price, Rating, Name.");
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class ToggleFeaturedDealCommandValidator : AbstractValidator<ToggleFeaturedDealCommand>
{
    public ToggleFeaturedDealCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Deal ID is required.");
    }
}
using FluentValidation;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Application.Validation;

public class UpdateDealCommandValidator : AbstractValidator<UpdateDealCommand>
{
    public UpdateDealCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Deal ID is required.");

        // First, validate that DealData is not null
        RuleFor(x => x.DealData)
            .NotNull().WithMessage("Deal data is required.");

        // Apply other validation rules only when DealData is not null
        When(x => x.DealData != null, () =>
        {
            RuleFor(x => x.DealData.Title)
                .NotEmpty().WithMessage("Deal title is required.")
                .MaximumLength(200).WithMessage("Deal title must not exceed 200 characters.");

            RuleFor(x => x.DealData.Description)
                .NotEmpty().WithMessage("Deal description is required.")
                .MaximumLength(2000).WithMessage("Deal description must not exceed 2000 characters.");

            RuleFor(x => x.DealData.RoomTypeId)
                .NotEmpty().WithMessage("Room type ID is required.");

            RuleFor(x => x.DealData.OriginalPrice)
                .GreaterThan(0).WithMessage("Original price must be greater than zero.")
                .LessThanOrEqualTo(100000).WithMessage("Original price must not exceed $100,000.");

            RuleFor(x => x.DealData.DiscountedPrice)
                .GreaterThan(0).WithMessage("Discounted price must be greater than zero.")
                .LessThanOrEqualTo(100000).WithMessage("Discounted price must not exceed $100,000.");

            RuleFor(x => x.DealData)
                .Must(x => x.DiscountedPrice < x.OriginalPrice)
                .WithMessage("Discounted price must be less than original price.")
                .WithName("DiscountedPrice");

            RuleFor(x => x.DealData.ValidFrom)
                .NotEmpty().WithMessage("Valid from date is required.");

            RuleFor(x => x.DealData.ValidTo)
                .NotEmpty().WithMessage("Valid to date is required.");

            RuleFor(x => x.DealData)
                .Must(x => x.ValidTo > x.ValidFrom)
                .WithMessage("Valid to date must be after valid from date.")
                .WithName("ValidTo");

            RuleFor(x => x.DealData.MaxBookings)
                .GreaterThan(0).WithMessage("Max bookings must be greater than zero.")
                .LessThanOrEqualTo(10000).WithMessage("Max bookings must not exceed 10,000.");

            RuleFor(x => x.DealData.ImageURL)
                .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.")
                .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Image URL must be a valid URL.")
                .When(x => !string.IsNullOrWhiteSpace(x.DealData.ImageURL));
        });
    }
}
using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Booking : AggregateRoot
{
    public DateTime CheckInDate { get; private set; }
    public DateTime CheckOutDate { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; } // Loose coupling - no navigation property

    // Navigation properties
    public Room Room { get; private set; } = null!;

    // For EF Core
    private Booking() { }

    public Booking(DateTime checkInDate, DateTime checkOutDate, Guid roomId, Guid userId)
    {
        if (checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");
        if (checkInDate.Date < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");
        if (roomId == Guid.Empty)
            throw new ArgumentException("Room ID cannot be empty.", nameof(roomId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        CheckInDate = checkInDate.Date; // Ensure we only store the date part
        CheckOutDate = checkOutDate.Date;
        RoomId = roomId;
        UserId = userId;
    }

    public void Update(DateTime checkInDate, DateTime checkOutDate)
    {
        if (checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");
        if (checkInDate.Date < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");

        CheckInDate = checkInDate.Date;
        CheckOutDate = checkOutDate.Date;
        MarkAsUpdated();
    }

    public int GetNumberOfNights()
    {
        return (CheckOutDate - CheckInDate).Days;
    }

    public bool OverlapsWith(DateTime otherCheckIn, DateTime otherCheckOut)
    {
        return CheckInDate < otherCheckOut && CheckOutDate > otherCheckIn;
    }

    public bool OverlapsWith(Booking otherBooking)
    {
        return OverlapsWith(otherBooking.CheckInDate, otherBooking.CheckOutDate);
    }
}
using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class City : AggregateRoot
{
    public string Name { get; private set; }
    public string Country { get; private set; }
    public string PostCode { get; private set; }

    // Navigation properties
    public ICollection<Hotel> Hotels { get; private set; } = new List<Hotel>();

    // For EF Core
    private City() { }

    public City(string name, string country, string postCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));
        if (string.IsNullOrWhiteSpace(postCode))
            throw new ArgumentException("Post code cannot be empty.", nameof(postCode));

        Name = name;
        Country = country;
        PostCode = postCode;
    }

    public void Update(string name, string country, string postCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));
        if (string.IsNullOrWhiteSpace(postCode))
            throw new ArgumentException("Post code cannot be empty.", nameof(postCode));

        Name = name;
        Country = country;
        PostCode = postCode;
        MarkAsUpdated();
    }
}
using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Deal : AggregateRoot
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Guid HotelId { get; private set; }
    public Guid? RoomTypeId { get; private set; }
    public decimal OriginalPrice { get; private set; }
    public decimal DiscountedPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public bool IsFeatured { get; private set; }
    public bool IsActive { get; private set; }
    public int MaxBookings { get; private set; }
    public int CurrentBookings { get; private set; }
    public string? ImageURL { get; private set; }

    // Navigation properties
    public Hotel Hotel { get; private set; } = null!;
    public RoomType? RoomType { get; private set; }

    // For EF Core
    private Deal() { }

    public Deal(string title, string description, Guid hotelId, decimal originalPrice, decimal discountedPrice,
               DateTime validFrom, DateTime validTo, bool isFeatured = false, Guid roomTypeId = default,
               int maxBookings = 100, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Deal title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Deal description cannot be empty.", nameof(description));
        if (hotelId == Guid.Empty)
            throw new ArgumentException("Hotel ID cannot be empty.", nameof(hotelId));
        if (roomTypeId == Guid.Empty)
            throw new ArgumentException("Room type ID cannot be empty.", nameof(roomTypeId));
        if (originalPrice <= 0)
            throw new ArgumentException("Original price must be greater than zero.", nameof(originalPrice));
        if (discountedPrice <= 0)
            throw new ArgumentException("Discounted price must be greater than zero.", nameof(discountedPrice));
        if (discountedPrice >= originalPrice)
            throw new ArgumentException("Discounted price must be less than original price.", nameof(discountedPrice));
        if (validFrom >= validTo)
            throw new ArgumentException("Valid from date must be before valid to date.", nameof(validFrom));
        if (maxBookings <= 0)
            throw new ArgumentException("Max bookings must be greater than zero.", nameof(maxBookings));

        Title = title;
        Description = description;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        OriginalPrice = originalPrice;
        DiscountedPrice = discountedPrice;
        DiscountPercentage = Math.Round(((originalPrice - discountedPrice) / originalPrice) * 100, 2);
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsFeatured = isFeatured;
        IsActive = true;
        MaxBookings = maxBookings;
        CurrentBookings = 0;
        ImageURL = imageUrl;
    }

    public void Update(string title, string description, Guid roomTypeId, decimal originalPrice, decimal discountedPrice,
                      DateTime validFrom, DateTime validTo, int maxBookings, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Deal title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Deal description cannot be empty.", nameof(description));
        if (roomTypeId == Guid.Empty)
            throw new ArgumentException("Room type ID cannot be empty.", nameof(roomTypeId));
        if (originalPrice <= 0)
            throw new ArgumentException("Original price must be greater than zero.", nameof(originalPrice));
        if (discountedPrice <= 0)
            throw new ArgumentException("Discounted price must be greater than zero.", nameof(discountedPrice));
        if (discountedPrice >= originalPrice)
            throw new ArgumentException("Discounted price must be less than original price.", nameof(discountedPrice));
        if (validFrom >= validTo)
            throw new ArgumentException("Valid from date must be before valid to date.", nameof(validFrom));
        if (maxBookings <= 0)
            throw new ArgumentException("Max bookings must be greater than zero.", nameof(maxBookings));

        Title = title;
        Description = description;
        RoomTypeId = roomTypeId;
        OriginalPrice = originalPrice;
        DiscountedPrice = discountedPrice;
        DiscountPercentage = Math.Round(((originalPrice - discountedPrice) / originalPrice) * 100, 2);
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxBookings = maxBookings;
        ImageURL = imageUrl;
        MarkAsUpdated();
    }

    public void ToggleFeatured()
    {
        IsFeatured = !IsFeatured;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public bool IsValid() => IsActive && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo;

    public bool IsAvailable() => IsValid() && CurrentBookings < MaxBookings;

    public void IncrementBookings()
    {
        if (!IsAvailable())
            throw new InvalidOperationException("Deal is not available for booking.");

        CurrentBookings++;
        MarkAsUpdated();
    }
}
using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Hotel : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Rating { get; private set; }
    public Guid CityId { get; private set; }

    // Navigation properties
    public City City { get; private set; } = null!;
    public ICollection<Room> Rooms { get; private set; } = new List<Room>();
    public ICollection<HotelImage> Images { get; private set; } = new List<HotelImage>(); // <-- ADD THIS LINE

    // For EF Core
    private Hotel() { }

    public Hotel(string name, string description, decimal rating, Guid cityId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));
        if (cityId == Guid.Empty)
            throw new ArgumentException("City ID cannot be empty.", nameof(cityId));

        Name = name;
        Description = description;
        Rating = rating;
        CityId = cityId;
    }

    public void Update(string name, string description, decimal rating, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));

        Name = name;
        Description = description;
        Rating = rating;
        MarkAsUpdated();
    }

    public void UpdateCity(Guid cityId)
    {
        if (cityId == Guid.Empty)
            throw new ArgumentException("City ID cannot be empty.", nameof(cityId));

        CityId = cityId;
        MarkAsUpdated();
    }
}
using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class HotelImage : AggregateRoot
{
    public Guid HotelId { get; private set; }
    public string Url { get; private set; }
    public string? Caption { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsCoverImage { get; private set; }

    // Navigation property
    public Hotel Hotel { get; private set; } = null!;

    private HotelImage() { } // For EF Core

    public HotelImage(Guid hotelId, string url, string? caption, int sortOrder, bool isCoverImage = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be empty.", nameof(url));
        if (hotelId == Guid.Empty)
            throw new ArgumentException("Hotel ID cannot be empty.", nameof(hotelId));

        HotelId = hotelId;
        Url = url;
        Caption = caption;
        SortOrder = sortOrder;
        IsCoverImage = isCoverImage;
    }

    public void SetAsCoverImage() => IsCoverImage = true;
    public void UnsetAsCoverImage() => IsCoverImage = false;
}
using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Room : AggregateRoot
{
    public string RoomNumber { get; private set; }
    public Guid HotelId { get; private set; }
    public Guid RoomTypeId { get; private set; }

    // Navigation properties
    public Hotel Hotel { get; private set; } = null!;
    public RoomType RoomType { get; private set; } = null!;
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    // For EF Core
    private Room() { }

    public Room(string roomNumber, Guid hotelId, Guid roomTypeId)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new ArgumentException("Room number cannot be empty.", nameof(roomNumber));
        if (hotelId == Guid.Empty)
            throw new ArgumentException("Hotel ID cannot be empty.", nameof(hotelId));
        if (roomTypeId == Guid.Empty)
            throw new ArgumentException("Room type ID cannot be empty.", nameof(roomTypeId));

        RoomNumber = roomNumber;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
    }

    public void Update(string roomNumber, Guid roomTypeId)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new ArgumentException("Room number cannot be empty.", nameof(roomNumber));
        if (roomTypeId == Guid.Empty)
            throw new ArgumentException("Room type ID cannot be empty.", nameof(roomTypeId));

        RoomNumber = roomNumber;
        RoomTypeId = roomTypeId;
        MarkAsUpdated();
    }

    public bool IsAvailableForPeriod(DateTime checkInDate, DateTime checkOutDate)
    {
        if (checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");

        return !Bookings.Any(booking =>
            booking.CheckInDate < checkOutDate && booking.CheckOutDate > checkInDate);
    }
}
using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class RoomType : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; } // <-- ADDED
    public decimal PricePerNight { get; private set; }
    public int MaxAdults { get; private set; }
    public int MaxChildren { get; private set; }
    public string? ImageUrl { get; private set; } // <-- ADDED

    // Navigation properties
    public ICollection<Room> Rooms { get; private set; } = new List<Room>();

    // For EF Core
    private RoomType() { }

    public RoomType(string name, string description, decimal pricePerNight, int maxAdults, int maxChildren, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Room type description cannot be empty.", nameof(description));
        if (pricePerNight < 0)
            throw new ArgumentException("Price per night cannot be negative.", nameof(pricePerNight));
        if (maxAdults <= 0)
            throw new ArgumentException("Max adults must be greater than zero.", nameof(maxAdults));
        if (maxChildren < 0)
            throw new ArgumentException("Max children cannot be negative.", nameof(maxChildren));

        Name = name;
        Description = description;
        PricePerNight = pricePerNight;
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
        ImageUrl = imageUrl;
    }

    public void Update(string name, string description, decimal pricePerNight, int maxAdults, int maxChildren, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Room type description cannot be empty.", nameof(description));
        if (pricePerNight < 0)
            throw new ArgumentException("Price per night cannot be negative.", nameof(pricePerNight));
        if (maxAdults <= 0)
            throw new ArgumentException("Max adults must be greater than zero.", nameof(maxAdults));
        if (maxChildren < 0)
            throw new ArgumentException("Max children cannot be negative.", nameof(maxChildren));

        Name = name;
        Description = description;
        PricePerNight = pricePerNight;
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
        ImageUrl = imageUrl;
        MarkAsUpdated();
    }

    public bool CanAccommodate(int adults, int children)
    {
        return adults <= MaxAdults && children <= MaxChildren;
    }
}
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("TravelBookingPlatform.Modules.Hotels.Domain")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Travel Booking Platform")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+93a496f363cd4aacb8bcf1ebace0e342d89deb01")]
[assembly: System.Reflection.AssemblyProductAttribute("TravelBookingPlatform.Modules.Hotels.Domain")]
[assembly: System.Reflection.AssemblyTitleAttribute("TravelBookingPlatform.Modules.Hotels.Domain")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.

// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("TravelBookingPlatform.Modules.Hotels.Domain")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Travel Booking Platform")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+a7c7800de87cfc47bdb963930610f98846c7bdc6")]
[assembly: System.Reflection.AssemblyProductAttribute("TravelBookingPlatform.Modules.Hotels.Domain")]
[assembly: System.Reflection.AssemblyTitleAttribute("TravelBookingPlatform.Modules.Hotels.Domain")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.

// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<Booking>> GetByRoomIdAsync(Guid roomId);
    Task<IReadOnlyList<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IReadOnlyList<Booking>> GetOverlappingBookingsAsync(
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate);
    Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate,
        Guid? excludeBookingId = null);
    Task<IReadOnlyList<Booking>> GetUserBookingsInDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate);
    Task<IReadOnlyList<Booking>> GetUpcomingBookingsAsync(Guid userId);
    Task<IReadOnlyList<Booking>> GetPastBookingsAsync(Guid userId);
    Task<bool> UserHasBookingInPeriodAsync(
        Guid userId,
        DateTime checkInDate,
        DateTime checkOutDate);
}
using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface ICityRepository : IRepository<City>
{
    Task<City?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<bool> PostCodeExistsAsync(string postCode, Guid? excludeId = null);

    // Search methods
    Task<IReadOnlyList<City>> GetCitySuggestionsAsync(string searchText, int maxResults = 10);
    Task<IReadOnlyList<City>> GetPopularDestinationsAsync(int count = 5);
    Task<IReadOnlyList<City>> SearchCitiesAsync(string searchText);
}
using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IDealRepository : IRepository<Deal>
{
    Task<List<Deal>> GetFeaturedDealsAsync(int count = 10);
    Task<List<Deal>> GetActiveDealsAsync();
    Task<List<Deal>> GetDealsByHotelAsync(Guid hotelId);
    Task<Deal?> GetDealWithDetailsAsync(Guid dealId);
    Task<bool> HasActiveDealForRoomTypeAsync(Guid roomTypeId);
    Task<Deal?> GetOverlappingDealAsync(Guid hotelId, Guid roomTypeId, DateTime validFrom, DateTime validTo);
    Task<Deal?> GetByHotelAndTitleAsync(Guid hotelId, string title);
    Task<int> GetActiveFeaturedDealsCountAsync(Guid hotelId);
}
using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.ValueObjects;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<IReadOnlyList<Hotel>> GetByCityIdAsync(Guid cityId);
    Task<IReadOnlyList<Hotel>> SearchByNameAsync(string searchTerm);
    Task<IReadOnlyList<Hotel>> SearchByCityNameAsync(string cityName);
    Task<IReadOnlyList<Hotel>> GetByRatingRangeAsync(decimal minRating, decimal maxRating);
    Task<Hotel?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<IReadOnlyList<Hotel>> GetHotelsWithAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        int adults,
        int children);
    Task<IReadOnlyList<Hotel>> SearchWithFiltersAsync(
        string? searchTerm = null,
        Guid? cityId = null,
        decimal? minRating = null,
        decimal? maxRating = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        int? adults = null,
        int? children = null);

    // New enhanced search methods
    Task<IReadOnlyList<Hotel>> GetHotelSuggestionsAsync(string searchText, int maxResults = 10);
    Task<(IReadOnlyList<Hotel> Hotels, int TotalCount)> SearchHotelsWithPaginationAsync(SearchCriteria criteria);

    // Hotel detail methods
    Task<Hotel?> GetHotelWithDetailsAsync(Guid id);
    Task<IReadOnlyList<Hotel>> GetHotelsWithDetailsAsync(IEnumerable<Guid> hotelIds);
    Task<Hotel?> GetHotelWithRoomsAndBookingsAsync(Guid hotelId);

    Task<Hotel?> GetHotelWithImagesAsync(Guid hotelId);
}
using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<IReadOnlyList<Room>> GetByHotelIdAsync(Guid hotelId);
    Task<IReadOnlyList<Room>> GetByRoomTypeIdAsync(Guid roomTypeId);
    Task<Room?> GetByHotelAndRoomNumberAsync(Guid hotelId, string roomNumber);
    Task<bool> RoomNumberExistsInHotelAsync(Guid hotelId, string roomNumber, Guid? excludeRoomId = null);
    Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate);
    Task<IReadOnlyList<Room>> GetAvailableRoomsForHotelAsync(
        Guid hotelId,
        DateTime checkInDate,
        DateTime checkOutDate);
    Task<IReadOnlyList<Room>> GetAvailableRoomsByTypeAsync(
        Guid roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate);
    Task<IReadOnlyList<Room>> GetAvailableRoomsWithCapacityAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        int adults,
        int children);

    Task<IReadOnlyList<Room>> GetAvailableRoomsByTypeForPeriodAsync(
        Guid hotelId,
        Guid roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate);
}
using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IRoomTypeRepository : IRepository<RoomType>
{
    Task<RoomType?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<IReadOnlyList<RoomType>> GetByCapacityAsync(int adults, int children);
    Task<IReadOnlyList<RoomType>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IReadOnlyList<RoomType>> GetWithCapacityAndPriceRangeAsync(
        int adults,
        int children,
        decimal? minPrice = null,
        decimal? maxPrice = null);
}
namespace TravelBookingPlatform.Modules.Hotels.Domain.ValueObjects;

public record SearchCriteria(
    string? SearchText,
    DateTime? CheckInDate,
    DateTime? CheckOutDate,
    int NumberOfRooms,
    int Adults,
    int Children,
    decimal? MinRating,
    decimal? MaxRating,
    Guid? CityId,
    int PageNumber,
    int PageSize,
    string SortBy = "Relevance")
{
    public static SearchCriteria Create(
        string? searchText = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        int numberOfRooms = 1,
        int adults = 2,
        int children = 0,
        decimal? minRating = null,
        decimal? maxRating = null,
        Guid? cityId = null,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Relevance")
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100.", nameof(pageSize));
        if (numberOfRooms < 1)
            throw new ArgumentException("Number of rooms must be at least 1.", nameof(numberOfRooms));
        if (adults < 1)
            throw new ArgumentException("Number of adults must be at least 1.", nameof(adults));
        if (children < 0)
            throw new ArgumentException("Number of children cannot be negative.", nameof(children));
        if (checkInDate.HasValue && checkOutDate.HasValue && checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");
        if (checkInDate.HasValue && checkInDate.Value.Date < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");
        if (minRating.HasValue && (minRating < 0 || minRating > 5))
            throw new ArgumentException("Minimum rating must be between 0 and 5.", nameof(minRating));
        if (maxRating.HasValue && (maxRating < 0 || maxRating > 5))
            throw new ArgumentException("Maximum rating must be between 0 and 5.", nameof(maxRating));
        if (minRating.HasValue && maxRating.HasValue && minRating > maxRating)
            throw new ArgumentException("Minimum rating cannot be greater than maximum rating.");

        return new SearchCriteria(
            searchText?.Trim(),
            checkInDate?.Date,
            checkOutDate?.Date,
            numberOfRooms,
            adults,
            children,
            minRating,
            maxRating,
            cityId,
            pageNumber,
            pageSize,
            sortBy);
    }

    public bool HasDateRange => CheckInDate.HasValue && CheckOutDate.HasValue;
    public bool HasRatingFilter => MinRating.HasValue || MaxRating.HasValue;
    public bool HasSearch => !string.IsNullOrWhiteSpace(SearchText);
    public bool HasGuestRequirements => Adults > 0 || Children > 0;
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure;

public static class HotelsInfrastructureDi
{
    public static IServiceCollection AddHotelsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register all repositories
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IDealRepository, DealRepository>();

        return services;
    }
}
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("TravelBookingPlatform.Modules.Hotels.Infrastructure")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Travel Booking Platform")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+93a496f363cd4aacb8bcf1ebace0e342d89deb01")]
[assembly: System.Reflection.AssemblyProductAttribute("TravelBookingPlatform.Modules.Hotels.Infrastructure")]
[assembly: System.Reflection.AssemblyTitleAttribute("TravelBookingPlatform.Modules.Hotels.Infrastructure")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.

// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("TravelBookingPlatform.Modules.Hotels.Infrastructure")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Travel Booking Platform")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+a7c7800de87cfc47bdb963930610f98846c7bdc6")]
[assembly: System.Reflection.AssemblyProductAttribute("TravelBookingPlatform.Modules.Hotels.Infrastructure")]
[assembly: System.Reflection.AssemblyTitleAttribute("TravelBookingPlatform.Modules.Hotels.Infrastructure")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.

// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Booking");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CheckInDate)
            .IsRequired()
            .HasColumnType("date"); // Store only date part

        builder.Property(b => b.CheckOutDate)
            .IsRequired()
            .HasColumnType("date"); // Store only date part

        builder.Property(b => b.RoomId)
            .IsRequired();

        builder.Property(b => b.UserId)
            .IsRequired(); // Loose coupling - no navigation property to User

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(b => b.RoomId)
            .HasDatabaseName("IX_Booking_RoomId");

        builder.HasIndex(b => b.UserId)
            .HasDatabaseName("IX_Booking_UserId");

        builder.HasIndex(b => b.CheckInDate)
            .HasDatabaseName("IX_Booking_CheckInDate");

        builder.HasIndex(b => b.CheckOutDate)
            .HasDatabaseName("IX_Booking_CheckOutDate");

        // Composite index for availability queries (most important for performance)
        builder.HasIndex(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate })
            .HasDatabaseName("IX_Booking_Room_Dates");

        // Index for user bookings queries
        builder.HasIndex(b => new { b.UserId, b.CheckInDate })
            .HasDatabaseName("IX_Booking_User_CheckIn");

        // Relationships
        builder.HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade); // Delete booking when room is deleted

        // Note: No foreign key constraint to User table (loose coupling)
        // UserId is stored as Guid but not enforced by database
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("City");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.PostCode)
            .IsRequired()
            .HasMaxLength(20);

        // unique constraints
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_City_Name_Unique");

        builder.HasIndex(c => c.PostCode)
            .IsUnique()
            .HasDatabaseName("IX_City_PostCode_Unique");

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.ToTable("Deal");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.OriginalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.DiscountedPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.DiscountPercentage)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(d => d.ValidFrom)
            .IsRequired();

        builder.Property(d => d.ValidTo)
            .IsRequired();

        builder.Property(d => d.IsFeatured)
            .IsRequired();

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.Property(d => d.MaxBookings)
            .IsRequired();

        builder.Property(d => d.CurrentBookings)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(d => d.ImageURL)
            .HasMaxLength(500);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(d => new { d.IsFeatured, d.IsActive, d.ValidTo })
            .HasDatabaseName("IX_Deal_Featured_Active_ValidTo");

        builder.HasIndex(d => d.HotelId)
            .HasDatabaseName("IX_Deal_HotelId");

        builder.HasIndex(d => d.RoomTypeId)
            .HasDatabaseName("IX_Deal_RoomTypeId");

        builder.HasIndex(d => new { d.IsActive, d.ValidTo })
            .HasDatabaseName("IX_Deal_Active_ValidTo");

        // Unique constraints for data integrity
        builder.HasIndex(d => new { d.HotelId, d.RoomTypeId, d.ValidFrom, d.ValidTo })
            .IsUnique()
            .HasDatabaseName("IX_Deal_Hotel_RoomType_DateRange_Unique")
            .HasFilter("[IsActive] = 1");

        builder.HasIndex(d => new { d.HotelId, d.Title })
            .IsUnique()
            .HasDatabaseName("IX_Deal_Hotel_Title_Unique")
            .HasFilter("[IsActive] = 1");

        // Relationships
        builder.HasOne(d => d.Hotel)
            .WithMany()
            .HasForeignKey(d => d.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.RoomType)
            .WithMany()
            .HasForeignKey(d => d.RoomTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotel");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(h => h.Rating)
            .IsRequired()
            .HasColumnType("decimal(3,2)"); // e.g., 4.75

        builder.Property(h => h.CityId)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(h => h.Name)
            .HasDatabaseName("IX_Hotel_Name");

        builder.HasIndex(h => h.CityId)
            .HasDatabaseName("IX_Hotel_CityId");

        builder.HasIndex(h => h.Rating)
            .HasDatabaseName("IX_Hotel_Rating");

        // Unique constraint for hotel name
        builder.HasIndex(h => h.Name)
            .IsUnique()
            .HasDatabaseName("IX_Hotel_Name_Unique");

        // Relationships
        builder.HasOne(h => h.City)
            .WithMany(c => c.Hotels)
            .HasForeignKey(h => h.CityId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting city if hotels exist

        builder.HasMany(h => h.Rooms)
            .WithOne(r => r.Hotel)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade); // Delete all rooms when hotel is deleted
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class HotelImageConfiguration : IEntityTypeConfiguration<HotelImage>
{
    public void Configure(EntityTypeBuilder<HotelImage> builder)
    {
        builder.ToTable("HotelImage");
        builder.HasKey(hi => hi.Id);
        builder.Property(hi => hi.Url).IsRequired().HasMaxLength(500);
        builder.Property(hi => hi.Caption).HasMaxLength(200);
        builder.Property(hi => hi.SortOrder).IsRequired();
        builder.Property(hi => hi.IsCoverImage).IsRequired();

        builder.HasOne(hi => hi.Hotel)
            .WithMany(h => h.Images)
            .HasForeignKey(hi => hi.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(hi => new { hi.HotelId, hi.SortOrder });
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Room");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.HotelId)
            .IsRequired();

        builder.Property(r => r.RoomTypeId)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(r => r.HotelId)
            .HasDatabaseName("IX_Room_HotelId");

        builder.HasIndex(r => r.RoomTypeId)
            .HasDatabaseName("IX_Room_RoomTypeId");

        builder.HasIndex(r => r.RoomNumber)
            .HasDatabaseName("IX_Room_RoomNumber");

        // Unique constraint: Room number must be unique within each hotel
        builder.HasIndex(r => new { r.HotelId, r.RoomNumber })
            .IsUnique()
            .HasDatabaseName("IX_Room_HotelId_RoomNumber_Unique");

        // Relationships
        builder.HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade); // Delete room when hotel is deleted

        builder.HasOne(r => r.RoomType)
            .WithMany(rt => rt.Rooms)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting room type if rooms exist

        builder.HasMany(r => r.Bookings)
            .WithOne(b => b.Room)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade); // Delete bookings when room is deleted
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomType");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(rt => rt.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(rt => rt.PricePerNight)
            .IsRequired()
            .HasColumnType("decimal(10,2)"); // e.g., 9999999.99

        builder.Property(rt => rt.MaxAdults)
            .IsRequired();

        builder.Property(rt => rt.MaxChildren)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(rt => rt.Name)
            .HasDatabaseName("IX_RoomType_Name");

        builder.HasIndex(rt => rt.PricePerNight)
            .HasDatabaseName("IX_RoomType_PricePerNight");

        builder.HasIndex(rt => new { rt.MaxAdults, rt.MaxChildren })
            .HasDatabaseName("IX_RoomType_Capacity");

        // Unique constraint for room type name
        builder.HasIndex(rt => rt.Name)
            .IsUnique()
            .HasDatabaseName("IX_RoomType_Name_Unique");

        // Relationships
        builder.HasMany(rt => rt.Rooms)
            .WithOne(r => r.RoomType)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting room type if rooms exist
    }
}
using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class BookingRepository : BaseRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(b => b.UserId == userId)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderByDescending(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetByRoomIdAsync(Guid roomId)
    {
        return await _dbSet
            .Where(b => b.RoomId == roomId)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.CheckInDate <= endDate && b.CheckOutDate >= startDate)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetOverlappingBookingsAsync(
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(b => b.RoomId == roomId &&
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate,
        Guid? excludeBookingId = null)
    {
        var query = _dbSet
            .Where(b => b.RoomId == roomId &&
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetUserBookingsInDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.UserId == userId &&
                b.CheckInDate <= endDate && b.CheckOutDate >= startDate)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetUpcomingBookingsAsync(Guid userId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(b => b.UserId == userId && b.CheckInDate >= today)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetPastBookingsAsync(Guid userId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(b => b.UserId == userId && b.CheckOutDate < today)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderByDescending(b => b.CheckOutDate)
            .ToListAsync();
    }

    public async Task<bool> UserHasBookingInPeriodAsync(
        Guid userId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .AnyAsync(b => b.UserId == userId &&
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate);
    }
}
using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class CityRepository : BaseRepository<City>, ICityRepository
{
    public CityRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<City?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(c => c.Name == name);
    }

    public async Task<bool> PostCodeExistsAsync(string postCode, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(c => c.PostCode == postCode);
    }

    public async Task<IReadOnlyList<City>> GetCitySuggestionsAsync(string searchText, int maxResults = 10)
    {
        var searchTerm = searchText.ToLower();
        return await _dbSet
            .Where(c => c.Name.ToLower().Contains(searchTerm) || c.Country.ToLower().Contains(searchTerm))
            .OrderBy(c => c.Name)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<City>> GetPopularDestinationsAsync(int count = 5)
    {
        // For now, return cities with the most hotels. In a real implementation,
        // this could be based on booking frequency, user searches, etc.
        return await _dbSet
            .Include(c => c.Hotels)
            .OrderByDescending(c => c.Hotels.Count)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<City>> SearchCitiesAsync(string searchText)
    {
        var searchTerm = searchText.ToLower();
        return await _dbSet
            .Where(c => c.Name.ToLower().Contains(searchTerm) || c.Country.ToLower().Contains(searchTerm))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class DealRepository : BaseRepository<Deal>, IDealRepository
{
    public DealRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<Deal>> GetFeaturedDealsAsync(int count = 10)
    {
        return await _dbSet
            .Where(d => d.IsFeatured && d.IsActive && d.ValidTo > DateTime.UtcNow)
            .Include(d => d.Hotel).ThenInclude(h => h.City)
            .Include(d => d.RoomType)
            .OrderBy(d => d.ValidTo)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Deal>> GetActiveDealsAsync()
    {
        return await _dbSet
            .Where(d => d.IsActive && d.ValidTo > DateTime.UtcNow)
            .Include(d => d.Hotel).ThenInclude(h => h.City)
            .Include(d => d.RoomType)
            .OrderBy(d => d.ValidTo)
            .ToListAsync();
    }

    public async Task<List<Deal>> GetDealsByHotelAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(d => d.HotelId == hotelId && d.IsActive)
            .Include(d => d.Hotel).ThenInclude(h => h.City)
            .Include(d => d.RoomType)
            .OrderBy(d => d.ValidTo)
            .ToListAsync();
    }

    public async Task<Deal?> GetDealWithDetailsAsync(Guid dealId)
    {
        return await _dbSet
            .Include(d => d.Hotel).ThenInclude(h => h.City)
            .Include(d => d.RoomType)
            .FirstOrDefaultAsync(d => d.Id == dealId);
    }

    public async Task<bool> HasActiveDealForRoomTypeAsync(Guid roomTypeId)
    {
        return await _dbSet
            .AnyAsync(d => d.RoomTypeId == roomTypeId && d.IsActive && d.ValidTo > DateTime.UtcNow);
    }

    public async Task<Deal?> GetOverlappingDealAsync(Guid hotelId, Guid roomTypeId, DateTime validFrom, DateTime validTo)
    {
        return await _dbSet
            .Where(d => d.HotelId == hotelId
                     && d.RoomTypeId == roomTypeId
                     && d.IsActive
                     && ((d.ValidFrom <= validFrom && d.ValidTo >= validFrom) ||
                         (d.ValidFrom <= validTo && d.ValidTo >= validTo) ||
                         (d.ValidFrom >= validFrom && d.ValidTo <= validTo)))
            .FirstOrDefaultAsync();
    }

    public async Task<Deal?> GetByHotelAndTitleAsync(Guid hotelId, string title)
    {
        return await _dbSet
            .Where(d => d.HotelId == hotelId
                     && d.Title == title
                     && d.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetActiveFeaturedDealsCountAsync(Guid hotelId)
    {
        return await _dbSet
            .CountAsync(d => d.HotelId == hotelId
                          && d.IsFeatured
                          && d.IsActive
                          && d.ValidTo > DateTime.UtcNow);
    }
}
using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.ValueObjects;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class HotelRepository : BaseRepository<Hotel>, IHotelRepository
{
    public HotelRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Hotel>> GetByCityIdAsync(Guid cityId)
    {
        return await _dbSet
            .Where(h => h.CityId == cityId)
            .Include(h => h.City)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> SearchByNameAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet
            .Where(h => h.Name.ToLower().Contains(lowerSearchTerm))
            .Include(h => h.City)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> SearchByCityNameAsync(string cityName)
    {
        var lowerCityName = cityName.ToLower();
        return await _dbSet
            .Where(h => h.City.Name.ToLower().Contains(lowerCityName))
            .Include(h => h.City)
            .OrderBy(h => h.City.Name)
            .ThenBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> GetByRatingRangeAsync(decimal minRating, decimal maxRating)
    {
        return await _dbSet
            .Where(h => h.Rating >= minRating && h.Rating <= maxRating)
            .Include(h => h.City)
            .OrderByDescending(h => h.Rating)
            .ToListAsync();
    }

    public async Task<Hotel?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Include(h => h.City)
            .FirstOrDefaultAsync(h => h.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(h => h.Id != excludeId.Value);
        }

        return await query.AnyAsync(h => h.Name == name);
    }

    public async Task<IReadOnlyList<Hotel>> GetHotelsWithAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        int adults,
        int children)
    {
        return await _dbSet
            .Where(h => h.Rooms.Any(r =>
                r.RoomType.MaxAdults >= adults &&
                r.RoomType.MaxChildren >= children &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate)))
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> SearchWithFiltersAsync(
        string? searchTerm = null,
        Guid? cityId = null,
        decimal? minRating = null,
        decimal? maxRating = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        int? adults = null,
        int? children = null)
    {
        var query = _dbSet.AsQueryable();

        // Text search (hotel name or city name) - case insensitive
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(lowerSearchTerm) ||
                h.City.Name.ToLower().Contains(lowerSearchTerm));
        }

        // City filter
        if (cityId.HasValue)
        {
            query = query.Where(h => h.CityId == cityId.Value);
        }

        // Rating filters
        if (minRating.HasValue)
        {
            query = query.Where(h => h.Rating >= minRating.Value);
        }
        if (maxRating.HasValue)
        {
            query = query.Where(h => h.Rating <= maxRating.Value);
        }

        // Availability and capacity filters
        if (checkInDate.HasValue && checkOutDate.HasValue && adults.HasValue && children.HasValue)
        {
            query = query.Where(h => h.Rooms.Any(r =>
                r.RoomType.MaxAdults >= adults.Value &&
                r.RoomType.MaxChildren >= children.Value &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate.Value && b.CheckOutDate > checkInDate.Value)));
        }

        return await query
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .OrderByDescending(h => h.Rating)
            .ThenBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> GetHotelSuggestionsAsync(string searchText, int maxResults = 10)
    {
        var searchTerm = searchText.ToLower();
        return await _dbSet
            .Where(h => h.Name.ToLower().Contains(searchTerm) || h.City.Name.ToLower().Contains(searchTerm))
            .Include(h => h.City)
            .OrderBy(h => h.Name)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Hotel> Hotels, int TotalCount)> SearchHotelsWithPaginationAsync(SearchCriteria criteria)
    {
        var query = _dbSet.AsQueryable();

        // Text search (hotel name or city name) - case insensitive
        if (criteria.HasSearch)
        {
            var searchTerm = criteria.SearchText!.ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(searchTerm) ||
                h.City.Name.ToLower().Contains(searchTerm));
        }

        // City filter
        if (criteria.CityId.HasValue)
        {
            query = query.Where(h => h.CityId == criteria.CityId.Value);
        }

        // Rating filters
        if (criteria.HasRatingFilter)
        {
            if (criteria.MinRating.HasValue)
            {
                query = query.Where(h => h.Rating >= criteria.MinRating.Value);
            }
            if (criteria.MaxRating.HasValue)
            {
                query = query.Where(h => h.Rating <= criteria.MaxRating.Value);
            }
        }

        // Availability and capacity filters
        if (criteria.HasDateRange && criteria.HasGuestRequirements)
        {
            query = query.Where(h => h.Rooms.Any(r =>
                r.RoomType.MaxAdults >= criteria.Adults &&
                r.RoomType.MaxChildren >= criteria.Children &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < criteria.CheckOutDate!.Value &&
                    b.CheckOutDate > criteria.CheckInDate!.Value)));
        }
        else if (criteria.HasGuestRequirements)
        {
            query = query.Where(h => h.Rooms.Any(r =>
                r.RoomType.MaxAdults >= criteria.Adults &&
                r.RoomType.MaxChildren >= criteria.Children));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = criteria.SortBy.ToLower() switch
        {
            "price" => query.OrderBy(h => h.Rooms.Min(r => r.RoomType.PricePerNight)),
            "rating" => query.OrderByDescending(h => h.Rating),
            "name" => query.OrderBy(h => h.Name),
            _ => query.OrderByDescending(h => h.Rating).ThenBy(h => h.Name) // Default: Relevance
        };

        // Apply pagination
        var hotels = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.Bookings)
            .Include(h => h.Images)
            .ToListAsync();

        return (hotels, totalCount);
    }

    public async Task<Hotel?> GetHotelWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Where(h => h.Id == id)
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .Include(h => h.Images)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<Hotel>> GetHotelsWithDetailsAsync(IEnumerable<Guid> hotelIds)
    {
        return await _dbSet
            .Where(h => hotelIds.Contains(h.Id))
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .Include(h => h.Images)
            .ToListAsync();
    }

    public async Task<Hotel?> GetHotelWithRoomsAndBookingsAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(h => h.Id == hotelId)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.Bookings)
            .Include(h => h.Images)
            .FirstOrDefaultAsync();
    }

    public async Task<Hotel?> GetHotelWithImagesAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(h => h.Id == hotelId)
            .Include(h => h.Images)
            .FirstOrDefaultAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class RoomRepository : BaseRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Room>> GetByHotelIdAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(r => r.HotelId == hotelId)
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> GetByRoomTypeIdAsync(Guid roomTypeId)
    {
        return await _dbSet
            .Where(r => r.RoomTypeId == roomTypeId)
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .OrderBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<Room?> GetByHotelAndRoomNumberAsync(Guid hotelId, string roomNumber)
    {
        return await _dbSet
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .FirstOrDefaultAsync(r => r.HotelId == hotelId && r.RoomNumber == roomNumber);
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsByTypeForPeriodAsync(
        Guid hotelId,
        Guid roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(r => r.HotelId == hotelId && r.RoomTypeId == roomTypeId)
            .Where(r => !r.Bookings.Any(b =>
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .ToListAsync();
    }

    public async Task<bool> RoomNumberExistsInHotelAsync(Guid hotelId, string roomNumber, Guid? excludeRoomId = null)
    {
        var query = _dbSet
            .Where(r => r.HotelId == hotelId && r.RoomNumber == roomNumber);

        if (excludeRoomId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRoomId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(r => !r.Bookings.Any(b =>
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .OrderBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsForHotelAsync(
        Guid hotelId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(r => r.HotelId == hotelId &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsByTypeAsync(
        Guid roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(r => r.RoomTypeId == roomTypeId &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .OrderBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsWithCapacityAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        int adults,
        int children)
    {
        return await _dbSet
            .Where(r => r.RoomType.MaxAdults >= adults &&
                r.RoomType.MaxChildren >= children &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .OrderBy(r => r.RoomType.PricePerNight)
            .ThenBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class RoomTypeRepository : BaseRepository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<RoomType?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(rt => rt.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(rt => rt.Id != excludeId.Value);
        }

        return await query.AnyAsync(rt => rt.Name == name);
    }

    public async Task<IReadOnlyList<RoomType>> GetByCapacityAsync(int adults, int children)
    {
        return await _dbSet
            .Where(rt => rt.MaxAdults >= adults && rt.MaxChildren >= children)
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<RoomType>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _dbSet
            .Where(rt => rt.PricePerNight >= minPrice && rt.PricePerNight <= maxPrice)
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<RoomType>> GetWithCapacityAndPriceRangeAsync(
        int adults,
        int children,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        var query = _dbSet
            .Where(rt => rt.MaxAdults >= adults && rt.MaxChildren >= children);

        if (minPrice.HasValue)
        {
            query = query.Where(rt => rt.PricePerNight >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(rt => rt.PricePerNight <= maxPrice.Value);
        }

        return await query
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();
    }
}
