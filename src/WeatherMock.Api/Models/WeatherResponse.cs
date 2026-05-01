namespace WeatherMock.Api.Models;

public sealed class WeatherResponse
{
    public decimal TemperatureC { get; set; }

    public DateTime MeasuredAtUtc { get; set; }
}