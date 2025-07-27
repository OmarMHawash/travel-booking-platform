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