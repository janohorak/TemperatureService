using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TemperatureService.Api.Models;
using TemperatureService.Api.Services;

namespace TemperatureService.Tests.Integration;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions,
                    TestAuthHandler>(
                    "Test",
                    _ => { });

            // Mock ITemperatureService
            var serviceMock = new Mock<ITemperatureService>();

            serviceMock
                .Setup(x => x.GetTemperatureAsync(
                    "bratislava",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TemperatureResult
                {
                    City = "bratislava",
                    TemperatureC = 22.5m,
                    MeasuredAtUtc = DateTime.UtcNow
                });

            services.AddScoped(_ => serviceMock.Object);
        });
    }
}