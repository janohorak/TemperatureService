namespace TemperatureService.Api.Options;

public sealed class WeatherApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}