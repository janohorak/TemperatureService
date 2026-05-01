using TemperatureService.Api.Cache;
using TemperatureService.Api.Clients;
using TemperatureService.Api.Infrastructure;
using TemperatureService.Api.Models;
using TemperatureService.Api.Infrastructure.Logging;

namespace TemperatureService.Api.Services;

public sealed class DefaultTemperatureService : ITemperatureService
{
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly ITemperatureCache _temperatureCache;
    private readonly ILogger<DefaultTemperatureService> _logger;

    public DefaultTemperatureService(
        IWeatherApiClient weatherApiClient,
        ITemperatureCache temperatureCache,
        ILogger<DefaultTemperatureService> logger)
    {
        _weatherApiClient = weatherApiClient;
        _temperatureCache = temperatureCache;
        _logger = logger;
    }

    public async Task<TemperatureResult?> GetTemperatureAsync(
        string city,
        CancellationToken cancellationToken)
    {
        if (!CityMapping.TryGetCityId(city, out var cityId))
        {
            _logger.LogWarning(
                TemperatureLogEvents.UnsupportedCity,
                "Pod {PodName}: city {City} is not supported.",
                RuntimeInfo.PodName,
                city);

            return null;
        }

        var normalizedCity = city.ToLowerInvariant();

        var cachedResult = await _temperatureCache.GetAsync(
            normalizedCity,
            cancellationToken);

        if (cachedResult != null)
        {
            _logger.LogInformation(
                TemperatureLogEvents.CacheHit,
                "Pod {PodName}: returning cached temperature for city {City}.",
                RuntimeInfo.PodName,
                normalizedCity);

            return cachedResult;
        }

        _logger.LogInformation(
            TemperatureLogEvents.CacheMiss,
            "Pod {PodName}: no cached temperature found for city {City}. Calling Weather API.",
            RuntimeInfo.PodName,
            normalizedCity);

        var weather = await _weatherApiClient.GetTemperatureAsync(
            cityId,
            cancellationToken);

        if (weather == null)
        {
            _logger.LogWarning(
                TemperatureLogEvents.WeatherApiUnavailable,
                "Pod {PodName}: Weather API unavailable and no cached temperature exists for city {City}.",
                RuntimeInfo.PodName,
                normalizedCity);

            return null;
        }

        var result = new TemperatureResult
        {
            City = normalizedCity,
            TemperatureC = Math.Round(weather.TemperatureC, 2),
            MeasuredAtUtc = weather.MeasuredAtUtc
        };

        await _temperatureCache.SetAsync(result, cancellationToken);

        _logger.LogInformation(
            TemperatureLogEvents.TemperatureCached,
            "Pod {PodName}: Weather API returned temperature for city {City}. Value stored in cache.",
            RuntimeInfo.PodName,
            normalizedCity);

        return result;
    }
}