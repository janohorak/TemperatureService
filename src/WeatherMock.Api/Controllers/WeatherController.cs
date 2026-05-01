using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherMock.Api.Models;

namespace WeatherMock.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    [HttpGet("{cityId:int}")]
    public async Task<IActionResult> GetWeather(
        int cityId,
        CancellationToken cancellationToken)
    {
        if (cityId is < 1 or > 4)
        {
            return NotFound(new
            {
                Message = $"CityId '{cityId}' is not supported."
            });
        }

        var random = Random.Shared.Next(1, 101);

        // ~10% simulated failure
        if (random <= 10)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Simulated Weather API failure."
            });
        }

        // ~10% simulated slow response
        if (random <= 20)
        {
            await Task.Delay(5000, cancellationToken);
        }

        var response = new WeatherResponse
        {
            TemperatureC = Math.Round(
                (decimal)(Random.Shared.NextDouble() * 30 - 5),
                2),

            MeasuredAtUtc = DateTime.UtcNow
        };

        return Ok(response);
    }
}