namespace TemperatureService.Api.Infrastructure;

public static class CityMapping
{
    private static readonly Dictionary<string, int> CityIds =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["bratislava"] = 1,
            ["praha"] = 2,
            ["budapest"] = 3,
            ["vieden"] = 4
        };

    public static bool TryGetCityId(string city, out int cityId)
    {
        return CityIds.TryGetValue(city, out cityId);
    }
}