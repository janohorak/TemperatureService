using Microsoft.Extensions.Options;
using TemperatureService.Api.Cache;
using TemperatureService.Api.Clients;
using TemperatureService.Api.Infrastructure;
using TemperatureService.Api.Locking;
using TemperatureService.Api.Models;
using TemperatureService.Api.Options;
using TemperatureService.Api.Infrastructure.Logging;

namespace TemperatureService.Api.Services;

public sealed class TemperatureRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TemperatureRefreshBackgroundService> _logger;
    private readonly TemperatureRefreshOptions _options;
    public TemperatureRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TemperatureRefreshBackgroundService> logger,
        IOptions<TemperatureRefreshOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            TemperatureLogEvents.RefreshServiceStarted,
            "Pod {PodName}: temperature refresh background service started.",
            RuntimeInfo.PodName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = GetDelayUntilNextRun(DateTimeOffset.UtcNow);

                _logger.LogInformation(
                    TemperatureLogEvents.RefreshScheduled,
                    "Pod {PodName}: next temperature refresh scheduled in {Delay}.",
                    RuntimeInfo.PodName,
                    delay);

                await Task.Delay(delay, stoppingToken);

                await RefreshCachedCitiesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Application is shutting down.
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    TemperatureLogEvents.RefreshServiceFailed,
                    ex,
                    "Pod {PodName}: temperature refresh background service failed.",
                    RuntimeInfo.PodName);
            }
        }
    }

    private async Task RefreshCachedCitiesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>();

        await using var lockHandle = await distributedLock.TryAcquireAsync(
            "temperature:refresh-lock",
            TimeSpan.FromMinutes(5),
            cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogInformation(
                TemperatureLogEvents.RefreshLockSkipped,
                "Pod {PodName}: temperature refresh skipped because another instance owns the lock.",
                RuntimeInfo.PodName);

            return;
        }

        var cache = scope.ServiceProvider.GetRequiredService<ITemperatureCache>();
        var weatherApiClient = scope.ServiceProvider.GetRequiredService<IWeatherApiClient>();

        var cachedCities = await cache.GetCachedCitiesAsync(cancellationToken);

        if (cachedCities.Count == 0)
        {
            _logger.LogInformation(
                TemperatureLogEvents.RefreshNoCachedCities,
                "Pod {PodName}: no cached cities found. Refresh skipped.",
                RuntimeInfo.PodName);

            return;
        }

        foreach (var city in cachedCities)
        {
            if (!CityMapping.TryGetCityId(city, out var cityId))
            {
                _logger.LogWarning(
                    TemperatureLogEvents.RefreshUnsupportedCity,
                    "Pod {PodName}: cached city {City} is not supported. Skipping refresh.",
                    RuntimeInfo.PodName,
                    city);

                continue;
            }

            var weather = await weatherApiClient.GetTemperatureAsync(
                cityId,
                cancellationToken);

            if (weather == null)
            {
                _logger.LogWarning(
                    TemperatureLogEvents.RefreshCityFailed,
                    "Pod {PodName}: Weather API unavailable for city {City}. Keeping previous cached value.",
                    RuntimeInfo.PodName,
                    city);

                continue;
            }

            var result = new TemperatureResult
            {
                City = city.ToLowerInvariant(),
                TemperatureC = Math.Round(weather.TemperatureC, 2),
                MeasuredAtUtc = weather.MeasuredAtUtc
            };

            await cache.SetAsync(result, cancellationToken);

            _logger.LogInformation(
                TemperatureLogEvents.RefreshCitySucceeded,
                "Pod {PodName}: cached temperature refreshed for city {City}.",
                RuntimeInfo.PodName,
                city);
        }
    }
    private TimeSpan GetDelayUntilNextRun(DateTimeOffset nowUtc)
    {
        if (_options.UseDevelopmentInterval)
        {
            return TimeSpan.FromSeconds(_options.DevelopmentIntervalSeconds);
        }

        var refreshHours = _options.RefreshHoursUtc
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        foreach (var hour in refreshHours)
        {
            var candidate = new DateTimeOffset(
                nowUtc.Year,
                nowUtc.Month,
                nowUtc.Day,
                hour,
                0,
                0,
                TimeSpan.Zero);

            if (candidate > nowUtc)
                return candidate - nowUtc;
        }

        var firstTomorrow = new DateTimeOffset(
            nowUtc.Year,
            nowUtc.Month,
            nowUtc.Day,
            refreshHours[0],
            0,
            0,
            TimeSpan.Zero).AddDays(1);

        return firstTomorrow - nowUtc;
    }
}