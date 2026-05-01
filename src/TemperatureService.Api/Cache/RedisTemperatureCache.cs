using System.Text.Json;
using StackExchange.Redis;
using TemperatureService.Api.Models;
using TemperatureService.Api.Infrastructure;
using TemperatureService.Api.Infrastructure.Logging;

namespace TemperatureService.Api.Cache;

public sealed class RedisTemperatureCache : ITemperatureCache
{
    private const string CitySetKey = "temperature:cities";

    private readonly IDatabase _database;
    private readonly ILogger<RedisTemperatureCache> _logger;

    public RedisTemperatureCache(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisTemperatureCache> logger)
    {
        _database = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task<TemperatureResult?> GetAsync(
        string city,
        CancellationToken cancellationToken)
    {
        var value = await _database.StringGetAsync(GetTemperatureKey(city));

        if (!value.HasValue)
            return null;

        try
        {
            return JsonSerializer.Deserialize<TemperatureResult>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(
                TemperatureLogEvents.CacheDeserializationFailed,
                ex,
                "Pod {PodName}: failed to deserialize cached temperature for city {City}.",
                RuntimeInfo.PodName,
                city);

            return null;
        }
    }

    public async Task SetAsync(
        TemperatureResult temperature,
        CancellationToken cancellationToken)
    {
        var normalizedCity = temperature.City.ToLowerInvariant();

        var json = JsonSerializer.Serialize(temperature);

        await _database.StringSetAsync(
            GetTemperatureKey(normalizedCity),
            json);

        await _database.SetAddAsync(
            CitySetKey,
            normalizedCity);
    }

    public async Task<IReadOnlyCollection<string>> GetCachedCitiesAsync(
        CancellationToken cancellationToken)
    {
        var values = await _database.SetMembersAsync(CitySetKey);

        return values
            .Where(x => x.HasValue)
            .Select(x => x.ToString())
            .ToList();
    }

    private static string GetTemperatureKey(string city)
        => $"temperature:{city.ToLowerInvariant()}";
}