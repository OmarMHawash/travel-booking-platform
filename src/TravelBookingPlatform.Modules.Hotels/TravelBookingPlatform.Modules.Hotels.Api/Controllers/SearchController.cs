using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SearchController> _logger;

    public SearchController(IMediator mediator, ILogger<SearchController> logger)
    {
        _mediator = mediator;
        _logger = logger;
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
    /// Get hotel suggestions for autocomplete (specific to hotels only)
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
    /// Get city suggestions for autocomplete (specific to cities only)
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
}