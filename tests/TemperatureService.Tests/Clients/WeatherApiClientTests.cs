using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using TemperatureService.Api.Clients;

namespace TemperatureService.Tests.Clients;

public class WeatherApiClientTests
{
    [Fact]
    public async Task GetTemperatureAsync_WhenCallerCancels_PropagatesCancellation()
    {
        var requestStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var httpClient = new HttpClient(new TestHttpMessageHandler(
            async (_, cancellationToken) =>
            {
                requestStarted.SetResult();
                await Task.Delay(Timeout.Infinite, cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }))
        {
            BaseAddress = new Uri("https://weather.example/")
        };
        var client = new WeatherApiClient(
            httpClient,
            NullLogger<WeatherApiClient>.Instance);
        using var cancellationTokenSource = new CancellationTokenSource();

        var request = client.GetTemperatureAsync(
            1,
            cancellationTokenSource.Token);

        await requestStarted.Task;
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => request);
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenWeatherApiReturnsNonSuccess_ReturnsNull()
    {
        using var httpClient = new HttpClient(new TestHttpMessageHandler(
            (_, _) => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))))
        {
            BaseAddress = new Uri("https://weather.example/")
        };
        var client = new WeatherApiClient(
            httpClient,
            NullLogger<WeatherApiClient>.Instance);

        var result = await client.GetTemperatureAsync(
            1,
            CancellationToken.None);

        Assert.Null(result);
    }

    private sealed class TestHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return sendAsync(request, cancellationToken);
        }
    }
}
