using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TemperatureService.Api.Models;
using Xunit;

namespace TemperatureService.Tests.Integration;

public class TemperatureApiIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TemperatureApiIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
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

    [Fact]
    public async Task GetTemperature_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/temperature/bratislava");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTemperature_WithUnsupportedCity_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/temperature/unknowncity");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}