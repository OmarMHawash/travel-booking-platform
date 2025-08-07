using Asp.Versioning;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IBookingRepository _bookingRepository;
    private readonly IFileStorageService _fileStorageService;

    public BookingsController(
        IMediator mediator,
        IMapper mapper,
        IBookingRepository bookingRepository,
        IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _mapper = mapper;
        _bookingRepository = bookingRepository;
        _fileStorageService = fileStorageService;
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
    /// [DEPRECATED] Creates a new booking for the authenticated user.
    /// </summary>
    /// <param name="requestDto">The details of the booking to create.</param>
    /// <returns>A confirmation of the created booking.</returns>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult CreateBooking([FromBody] CreateBookingRequestDto requestDto)
    {
        // This endpoint is now replaced by the /initiate flow and should not be used.
        // It will be removed in a future version.
        return new ObjectResult("This endpoint is deprecated. Please use POST /api/v1/bookings/initiate.")
        {
            StatusCode = StatusCodes.Status410Gone
        };
    }

    /// <summary>
    /// Creates a new review for a completed booking.
    /// </summary>
    /// <param name="bookingId">The ID of the booking to review.</param>
    /// <param name="requestDto">The review content, including star rating and description.</param>
    /// <returns>The created review.</returns>
    [HttpPost("{bookingId}/reviews")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReview(
        [FromRoute] Guid bookingId,
        [FromBody] CreateReviewRequestDto requestDto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "Invalid user identifier in token." });
        }

        var command = new CreateReviewCommand(
            bookingId,
            userId,
            requestDto.StarRating,
            requestDto.Description);

        var result = await _mediator.Send(command);

        return CreatedAtAction(
            "GetHotelReviews",
            "Hotels",
            new { hotelId = result.Id, version = "1.0" },
            result);
    }

    /// <summary>
    /// Initiates a new booking, creating it in a 'PendingPayment' state.
    /// </summary>
    /// <param name="requestDto">The details of the booking to initiate.</param>
    /// <returns>A booking ID and a client secret for the payment gateway.</returns>
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(InitiateBookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InitiateBooking([FromBody] InitiateBookingRequestDto requestDto)
    {

        if (requestDto == null)
        {
            return BadRequest(new { Message = "please provide a valid request body" });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "Invalid user identifier in token." });
        }

        var command = _mapper.Map<InitiateBookingCommand>(requestDto);
        command.UserId = userId;

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Gets the confirmation details for a completed booking.
    /// </summary>
    /// <param name="id">The ID of the booking.</param>
    /// <returns>Rich details of the confirmed booking.</returns>
    [HttpGet("{id}/confirmation")]
    [ProducesResponseType(typeof(BookingDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingConfirmation(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "Invalid user identifier in token." });
        }

        var query = new GetBookingConfirmationQuery(id, userId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Downloads the PDF confirmation for a booking.
    /// </summary>
    /// <param name="id">The ID of the booking.</param>
    /// <returns>A PDF file of the booking confirmation.</returns>
    [HttpGet("{id}/confirmation/download-pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadBookingConfirmationPdf(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "Invalid user identifier in token." });
        }

        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking is null || booking.UserId != userId)
        {
            return NotFound(new { Message = "Booking not found." });
        }

        if (booking.PdfGenerationFailed)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Message = "We encountered an issue generating your confirmation PDF. Please try again later." });
        }

        if (string.IsNullOrEmpty(booking.ConfirmationPdfUrl))
        {
            return Accepted(new { Message = "Your booking confirmation is still being generated. Please check back in a moment." });
        }

        var fileBytes = await _fileStorageService.GetFileAsync(booking.ConfirmationPdfUrl);
        if (fileBytes is null)
        {
            return NotFound(new { Message = "Booking confirmation PDF not found. It may have been moved or deleted." });
        }

        return File(fileBytes, "application/pdf", $"booking-confirmation-{id}.pdf");
    }
}