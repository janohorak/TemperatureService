using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TemperatureService.Api.Models;
using Xunit;

namespace TemperatureService.Tests.Integration;

public class TemperatureApiIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TemperatureApiIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Fact]
    public async Task GetTemperature_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/temperature/bratislava");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<TemperatureResult>();

        Assert.NotNull(result);
        Assert.Equal("bratislava", result.City);
        Assert.Equal(22.5m, result.TemperatureC);
    }
}