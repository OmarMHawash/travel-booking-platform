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
public class DealsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DealsController(IMediator mediator)
    {
        _mediator = mediator;
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
            return BadRequest(new { Message = "Count must be between 1 and 50." });
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
            return BadRequest(new { Message = "Page number must be greater than 0." });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { Message = "Page size must be between 1 and 100." });
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
            return BadRequest(new { Message = "Invalid deal ID provided." });
        }

        var query = new GetDealByIdQuery(id);
        var deal = await _mediator.Send(query);

        if (deal == null)
        {
            return NotFound(new { Message = $"Deal with ID {id} not found." });
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
    public async Task<IActionResult> CreateDeal([FromBody] CreateDealDto dealDto)
    {
        var command = new CreateDealCommand(dealDto);
        var dealId = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetDealById), new { id = dealId }, dealId);
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
            return BadRequest(new { Message = "Invalid deal ID provided." });
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
            return BadRequest(new { Message = "Invalid deal ID provided." });
        }

        var command = new ToggleFeaturedDealCommand(id);
        await _mediator.Send(command);

        return NoContent();
    }
}