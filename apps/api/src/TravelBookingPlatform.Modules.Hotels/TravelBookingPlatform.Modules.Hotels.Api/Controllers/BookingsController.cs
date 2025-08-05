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