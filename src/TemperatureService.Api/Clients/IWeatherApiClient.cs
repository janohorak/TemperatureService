using TemperatureService.Api.Models;

namespace TemperatureService.Api.Clients;

public interface IWeatherApiClient
{
    Task<WeatherApiResponse?> GetTemperatureAsync(int cityId, CancellationToken cancellationToken);
}