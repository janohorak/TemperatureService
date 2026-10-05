using TemperatureService.Api.Models;
using TemperatureService.Api.Infrastructure;
using TemperatureService.Api.Infrastructure.Logging;

namespace TemperatureService.Api.Clients;

public sealed class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherApiClient> _logger;

    public WeatherApiClient(
        HttpClient httpClient,
        ILogger<WeatherApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WeatherApiResponse?> GetTemperatureAsync(
        int cityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                cityId.ToString(),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    TemperatureLogEvents.WeatherApiNonSuccessStatusCode,
                    "Pod {PodName}: Weather API returned non-success status code. CityId: {CityId}, StatusCode: {StatusCode}.",
                    RuntimeInfo.PodName,
                    cityId,
                    response.StatusCode);

                return null;
            }

            return await response.Content.ReadFromJsonAsync<WeatherApiResponse>(
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                TemperatureLogEvents.WeatherApiRequestFailed,
                ex,
                "Pod {PodName}: Weather API request failed. CityId: {CityId}.",
                RuntimeInfo.PodName,
                cityId);

            return null;
        }
    }
}