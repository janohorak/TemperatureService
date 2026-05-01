namespace TemperatureService.Api.Options;

public sealed class TemperatureRefreshOptions
{
    public bool UseDevelopmentInterval { get; set; }

    public int DevelopmentIntervalSeconds { get; set; } = 60;

    public int[] RefreshHoursUtc { get; set; } = [9, 16];
}