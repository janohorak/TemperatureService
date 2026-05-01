using System.Collections.Concurrent;
using TemperatureService.Api.Models;

namespace TemperatureService.Api.Cache;

public sealed class InMemoryTemperatureCache : ITemperatureCache
{
    private readonly ConcurrentDictionary<string, TemperatureResult> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<TemperatureResult?> GetAsync(
        string city,
        CancellationToken cancellationToken)
    {
        _cache.TryGetValue(city, out var result);
        return Task.FromResult(result);
    }

    public Task SetAsync(
        TemperatureResult temperature,
        CancellationToken cancellationToken)
    {
        _cache[temperature.City] = temperature;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> GetCachedCitiesAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string> cities = _cache.Keys.ToList();
        return Task.FromResult(cities);
    }
}