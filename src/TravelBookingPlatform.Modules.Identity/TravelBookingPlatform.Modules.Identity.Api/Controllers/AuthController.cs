using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;

namespace TravelBookingPlatform.Modules.Identity.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="command">User registration details.</param>
    /// <returns>The created user details.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        try
        {
            var userDto = await _mediator.Send(command);
            return CreatedAtAction(nameof(Register), new { id = userDto.Id }, userDto);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new
            {
                Property = e.PropertyName,
                Error = e.ErrorMessage
            });
            return BadRequest(new { message = "Validation failed", errors = errors });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="command">User login details.</param>
    /// <returns>Authentication response with token and user details.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        try
        {
            var authResponse = await _mediator.Send(command);
            return Ok(authResponse);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new
            {
                Property = e.PropertyName,
                Error = e.ErrorMessage
            });
            return BadRequest(new { message = "Validation failed", errors = errors });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}