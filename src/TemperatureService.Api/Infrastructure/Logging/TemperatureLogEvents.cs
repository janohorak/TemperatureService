using Microsoft.Extensions.Logging;

namespace TemperatureService.Api.Infrastructure.Logging;

public static class TemperatureLogEvents
{
    // Temperature service
    public static readonly EventId UnsupportedCity =
        new(1000, nameof(UnsupportedCity));

    public static readonly EventId CacheHit =
        new(1001, nameof(CacheHit));

    public static readonly EventId CacheMiss =
        new(1002, nameof(CacheMiss));

    public static readonly EventId TemperatureCached =
        new(1003, nameof(TemperatureCached));

    public static readonly EventId WeatherApiUnavailable =
        new(1004, nameof(WeatherApiUnavailable));

    // Background refresh
    public static readonly EventId RefreshServiceStarted =
        new(2000, nameof(RefreshServiceStarted));

    public static readonly EventId RefreshScheduled =
        new(2001, nameof(RefreshScheduled));

    public static readonly EventId RefreshLockSkipped =
        new(2002, nameof(RefreshLockSkipped));

    public static readonly EventId RefreshNoCachedCities =
        new(2003, nameof(RefreshNoCachedCities));

    public static readonly EventId RefreshUnsupportedCity =
        new(2004, nameof(RefreshUnsupportedCity));

    public static readonly EventId RefreshCityFailed =
        new(2005, nameof(RefreshCityFailed));

    public static readonly EventId RefreshCitySucceeded =
        new(2006, nameof(RefreshCitySucceeded));

    public static readonly EventId RefreshServiceFailed =
        new(2007, nameof(RefreshServiceFailed));

    // Weather API client
    public static readonly EventId WeatherApiNonSuccessStatusCode =
        new(3000, nameof(WeatherApiNonSuccessStatusCode));

    public static readonly EventId WeatherApiRequestFailed =
        new(3001, nameof(WeatherApiRequestFailed));

    // Redis cache
    public static readonly EventId CacheDeserializationFailed =
        new(4000, nameof(CacheDeserializationFailed));

    // Redis distributed lock
    public static readonly EventId DistributedLockAcquired =
        new(5000, nameof(DistributedLockAcquired));

    public static readonly EventId DistributedLockNotAcquired =
        new(5001, nameof(DistributedLockNotAcquired));

    public static readonly EventId DistributedLockReleased =
        new(5002, nameof(DistributedLockReleased));

    public static readonly EventId DistributedLockReleaseFailed =
        new(5003, nameof(DistributedLockReleaseFailed));
}