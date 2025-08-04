using Microsoft.AspNetCore.Mvc;

namespace TravelBookingPlatform.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly ILogger<SystemController> _logger;

    public SystemController(ILogger<SystemController> logger)
    {
        _logger = logger;
    }

    [HttpGet("info")]
    public IActionResult GetSystemInfo()
    {
        _logger.LogInformation("Getting system information.");
        return Ok(new
        {
            ApplicationName = "TravelBookingPlatform",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Version = "0.1.0",
            CurrentTime = DateTimeOffset.UtcNow
        });
    }
}