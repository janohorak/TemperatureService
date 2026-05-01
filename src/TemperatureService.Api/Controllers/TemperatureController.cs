using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemperatureService.Api.Services;

namespace TemperatureService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TemperatureController : ControllerBase
{
    private readonly ITemperatureService _temperatureService;

    public TemperatureController(ITemperatureService temperatureService)
    {
        _temperatureService = temperatureService;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> GetTemperature(
        string city,
        CancellationToken cancellationToken)
    {
        var result = await _temperatureService.GetTemperatureAsync(
            city,
            cancellationToken);

        if (result == null)
        {
            return NotFound(new
            {
                Message = $"City '{city}' is not supported or data unavailable."
            });
        }

        return Ok(result);
    }
}