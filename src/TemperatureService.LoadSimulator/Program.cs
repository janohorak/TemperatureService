using System.Diagnostics;
using System.Net.Http.Headers;

var baseUrl = GetArg(args, "--url") ?? "http://localhost";
var token = GetArg(args, "--token") ?? "temperature-api-secret-token";
var workers = int.TryParse(GetArg(args, "--workers"), out var parsedWorkers)
    ? parsedWorkers
    : 10;

var delayMs = int.TryParse(GetArg(args, "--delay"), out var parsedDelay)
    ? parsedDelay
    : 500;

var cities = new[]
{
    "bratislava",
    "praha",
    "zvolen",
    "budapest",
    "vieden",    
    "zilina"
};

Console.WriteLine("Temperature Service Load Simulator");
Console.WriteLine($"Base URL: {baseUrl}");
Console.WriteLine($"Workers: {workers}");
Console.WriteLine($"Delay: {delayMs} ms");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};

httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

var tasks = Enumerable
    .Range(1, workers)
    .Select(workerId => RunWorkerAsync(
        workerId,
        httpClient,
        cities,
        delayMs,
        cancellationTokenSource.Token))
    .ToArray();

await Task.WhenAll(tasks);

static async Task RunWorkerAsync(
    int workerId,
    HttpClient httpClient,
    string[] cities,
    int delayMs,
    CancellationToken cancellationToken)
{
    var random = new Random(workerId);

    while (!cancellationToken.IsCancellationRequested)
    {
        var city = cities[random.Next(cities.Length)];
        var path = $"/api/temperature/{city}";

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await httpClient.GetAsync(
                path,
                cancellationToken);

            stopwatch.Stop();

            Console.WriteLine(
                $"[{DateTimeOffset.Now:HH:mm:ss}] Worker {workerId} | {city} | HTTP {(int)response.StatusCode} | {stopwatch.ElapsedMilliseconds} ms");

        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Console.WriteLine(
                $"[{DateTimeOffset.Now:HH:mm:ss}] Worker {workerId} | {city} | ERROR | {stopwatch.ElapsedMilliseconds} ms | {ex.Message}");
        }

        await Task.Delay(delayMs, cancellationToken);
    }
}

static string? GetArg(string[] args, string name)
{
    var index = Array.IndexOf(args, name);

    if (index < 0 || index + 1 >= args.Length)
        return null;

    return args[index + 1];
}