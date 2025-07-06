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
        // MediatR will automatically find the CreateCityCommandHandler and apply FluentValidation (if configured globally)
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

    // Other CRUD operations (GetById, Update, Delete) would follow a similar pattern
}