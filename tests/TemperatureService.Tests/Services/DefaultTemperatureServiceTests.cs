using Microsoft.Extensions.Logging;
using Moq;
using TemperatureService.Api.Cache;
using TemperatureService.Api.Clients;
using TemperatureService.Api.Models;
using TemperatureService.Api.Services;
using Xunit;

namespace TemperatureService.Tests.Services;

public class DefaultTemperatureServiceTests
{
    private readonly Mock<IWeatherApiClient> _weatherApiClientMock;
    private readonly Mock<ITemperatureCache> _temperatureCacheMock;
    private readonly Mock<ILogger<DefaultTemperatureService>> _loggerMock;

    private readonly DefaultTemperatureService _service;

    public DefaultTemperatureServiceTests()
    {
        _weatherApiClientMock = new Mock<IWeatherApiClient>();
        _temperatureCacheMock = new Mock<ITemperatureCache>();
        _loggerMock = new Mock<ILogger<DefaultTemperatureService>>();

        _service = new DefaultTemperatureService(
            _weatherApiClientMock.Object,
            _temperatureCacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenCityIsNotSupported_ReturnsNull()
    {
        // Arrange
        var city = "UnknownCity";

        // Act
        var result = await _service.GetTemperatureAsync(
            city,
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _temperatureCacheMock.Verify(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _weatherApiClientMock.Verify(x => x.GetTemperatureAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenCacheContainsValue_ReturnsCachedResult()
    {
        // Arrange
        var city = "Bratislava";

        var cachedResult = new TemperatureResult
        {
            City = "bratislava",
            TemperatureC = 21.50m,
            MeasuredAtUtc = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        _temperatureCacheMock
            .Setup(x => x.GetAsync(
                "bratislava",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);

        // Act
        var result = await _service.GetTemperatureAsync(
            city,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("bratislava", result.City);
        Assert.Equal(21.50m, result.TemperatureC);

        _weatherApiClientMock.Verify(x => x.GetTemperatureAsync(
                1,
                It.IsAny<CancellationToken>()),
            Times.Never);

        _temperatureCacheMock.Verify(x => x.SetAsync(
                It.IsAny<TemperatureResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenCacheIsEmptyAndWeatherApiReturnsValue_ReturnsApiResultAndStoresItInCache()
    {
        // Arrange
        var city = "Bratislava";

        var measuredAtUtc = new DateTime(
            2026,
            5,
            1,
            10,
            0,
            0,
            DateTimeKind.Utc);

        var weatherApiResponse = new WeatherApiResponse
        {
            TemperatureC = 22.456m,
            MeasuredAtUtc = measuredAtUtc
        };

        _temperatureCacheMock
            .Setup(x => x.GetAsync(
                "bratislava",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemperatureResult?)null);

        _weatherApiClientMock
            .Setup(x => x.GetTemperatureAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherApiResponse);

        // Act
        var result = await _service.GetTemperatureAsync(
            city,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("bratislava", result.City);
        Assert.Equal(22.46m, result.TemperatureC);
        Assert.Equal(measuredAtUtc, result.MeasuredAtUtc);

        _weatherApiClientMock.Verify(x => x.GetTemperatureAsync(
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _temperatureCacheMock.Verify(x => x.SetAsync(
                It.Is<TemperatureResult>(r =>
                    r.City == "bratislava" &&
                    r.TemperatureC == 22.46m &&
                    r.MeasuredAtUtc == measuredAtUtc),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenCacheIsEmptyAndWeatherApiReturnsNull_ReturnsNull()
    {
        // Arrange
        var city = "Bratislava";

        _temperatureCacheMock
            .Setup(x => x.GetAsync(
                "bratislava",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemperatureResult?)null);

        _weatherApiClientMock
            .Setup(x => x.GetTemperatureAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherApiResponse?)null);

        // Act
        var result = await _service.GetTemperatureAsync(
            city,
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _weatherApiClientMock.Verify(x => x.GetTemperatureAsync(
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _temperatureCacheMock.Verify(x => x.SetAsync(
                It.IsAny<TemperatureResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

}