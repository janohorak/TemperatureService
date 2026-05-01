namespace TemperatureService.Api.Models;

public sealed class TemperatureResult
{
    public string City { get; set; } = string.Empty;

    public decimal TemperatureC { get; set; }

    public DateTime MeasuredAtUtc { get; set; }
}