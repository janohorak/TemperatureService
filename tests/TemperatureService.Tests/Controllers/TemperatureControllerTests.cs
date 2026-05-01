using Microsoft.AspNetCore.Mvc;
using Moq;
using TemperatureService.Api.Controllers;
using TemperatureService.Api.Models;
using TemperatureService.Api.Services;

namespace TemperatureService.Tests.Controllers;

public class TemperatureControllerTests
{
    private readonly Mock<ITemperatureService> _temperatureServiceMock;
    private readonly TemperatureController _controller;

    public TemperatureControllerTests()
    {
        _temperatureServiceMock = new Mock<ITemperatureService>();

        _controller = new TemperatureController(
            _temperatureServiceMock.Object);
    }

    [Fact]
    public async Task GetTemperature_WhenServiceReturnsTemperature_ReturnsOkResult()
    {
        // Arrange
        var city = "Bratislava";

        var temperatureResult = new TemperatureResult
        {
            City = "bratislava",
            TemperatureC = 22.45m,
            MeasuredAtUtc = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        _temperatureServiceMock
            .Setup(x => x.GetTemperatureAsync(
                city,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperatureResult);

        // Act
        var result = await _controller.GetTemperature(
            city,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<TemperatureResult>(okResult.Value);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal("bratislava", value.City);
        Assert.Equal(22.45m, value.TemperatureC);
        Assert.Equal(temperatureResult.MeasuredAtUtc, value.MeasuredAtUtc);
    }

    [Fact]
    public async Task GetTemperature_WhenServiceReturnsNull_ReturnsNotFoundResult()
    {
        // Arrange
        var city = "UnknownCity";

        _temperatureServiceMock
            .Setup(x => x.GetTemperatureAsync(
                city,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemperatureResult?)null);

        // Act
        var result = await _controller.GetTemperature(
            city,
            CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task GetTemperature_CallsServiceWithCorrectCity()
    {
        // Arrange
        var city = "Bratislava";

        _temperatureServiceMock
            .Setup(x => x.GetTemperatureAsync(
                city,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TemperatureResult
            {
                City = "bratislava",
                TemperatureC = 20,
                MeasuredAtUtc = DateTime.UtcNow
            });

        // Act
        await _controller.GetTemperature(
            city,
            CancellationToken.None);

        // Assert
        _temperatureServiceMock.Verify(x => x.GetTemperatureAsync(
                city,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}