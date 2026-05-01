namespace TemperatureService.Api.Models;

public sealed class WeatherApiResponse
{
    public decimal TemperatureC { get; set; }

    public DateTime MeasuredAtUtc { get; set; }
}