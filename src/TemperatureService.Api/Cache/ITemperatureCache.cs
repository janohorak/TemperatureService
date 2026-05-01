using TemperatureService.Api.Models;

namespace TemperatureService.Api.Cache;

public interface ITemperatureCache
{
    Task<TemperatureResult?> GetAsync(
        string city,
        CancellationToken cancellationToken);

    Task SetAsync(
        TemperatureResult temperature,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetCachedCitiesAsync(
        CancellationToken cancellationToken);
}