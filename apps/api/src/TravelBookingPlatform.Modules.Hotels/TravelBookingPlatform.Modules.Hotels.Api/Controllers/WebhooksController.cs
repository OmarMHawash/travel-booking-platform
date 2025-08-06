using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;

namespace TravelBookingPlatform.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public WebhooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Endpoint for receiving Stripe webhook events.
    /// </summary>
    [HttpPost("stripe")]
    [AllowAnonymous] // Stripe servers don't use JWT. Security is via signature validation.
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];

        var command = new ProcessStripeWebhookCommand(json, stripeSignature!);
        await _mediator.Send(command);

        return Ok();
    }
}