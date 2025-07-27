using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Application.Queries;

namespace TravelBookingPlatform.Modules.Identity.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <returns>The current user's details.</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new BusinessValidationException("Invalid user ID", "UserId");
        }

        var user = await _mediator.Send(new GetUserByIdQuery { UserId = userGuid });
        if (user == null)
        {
            throw new BusinessValidationException("User not found", "UserId");
        }

        return Ok(user);
    }

    /// <summary>
    /// Gets a user by ID (Admin only).
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <returns>The user details.</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUser(string id)
    {
        // Override automatic model state validation to use our custom format
        if (!ModelState.IsValid)
        {
            throw new BusinessValidationException("Invalid user ID format", "Id");
        }

        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var userGuid))
        {
            throw new BusinessValidationException("Invalid user ID format", "Id");
        }

        var user = await _mediator.Send(new GetUserByIdQuery { UserId = userGuid });
        if (user == null)
        {
            throw new BusinessValidationException("User not found", "Id");
        }

        return Ok(user);
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="command">Password change details.</param>
    /// <returns>Success response.</returns>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new BusinessValidationException("Invalid user ID", "UserId");
        }

        command.UserId = userGuid;
        await _mediator.Send(command);
        return Ok(new { message = "Password changed successfully" });
    }
}