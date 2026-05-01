using TemperatureService.Api.Models;

namespace TemperatureService.Api.Services;

public interface ITemperatureService
{
    Task<TemperatureResult?> GetTemperatureAsync(
        string city,
        CancellationToken cancellationToken);
}
