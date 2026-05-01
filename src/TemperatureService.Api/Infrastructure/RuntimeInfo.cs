namespace TemperatureService.Api.Infrastructure;

public static class RuntimeInfo
{
    public static string PodName =>
        Environment.GetEnvironmentVariable("POD_NAME") ?? "local";
}