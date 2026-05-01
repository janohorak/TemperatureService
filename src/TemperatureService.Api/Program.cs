using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Net.Http.Headers;
using TemperatureService.Api.Auth;
using TemperatureService.Api.Cache;
using TemperatureService.Api.Clients;
using TemperatureService.Api.Locking;
using TemperatureService.Api.Options;
using TemperatureService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


builder.Services.Configure<WeatherApiOptions>(
    builder.Configuration.GetSection("WeatherApi"));

builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>((sp, client) =>
{
    var options = sp
        .GetRequiredService<IOptions<WeatherApiOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", options.Token);

    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = sp
        .GetRequiredService<IOptions<RedisOptions>>()
        .Value;

    return ConnectionMultiplexer.Connect(options.ConnectionString);
});



builder.Services.Configure<ApiAuthOptions>(
    builder.Configuration.GetSection("ApiAuth"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ApiAuthOptions>>().Value);

builder.Services
    .AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, BearerTokenAuthenticationHandler>(
        "Bearer",
        _ => { });

builder.Services.AddAuthorization();

builder.Services.Configure<TemperatureRefreshOptions>(
    builder.Configuration.GetSection("TemperatureRefresh"));

//builder.Services.AddSingleton<ITemperatureCache, InMemoryTemperatureCache>();
builder.Services.AddSingleton<ITemperatureCache, RedisTemperatureCache>();
builder.Services.AddScoped<ITemperatureService, DefaultTemperatureService>();
builder.Services.AddHostedService<TemperatureRefreshBackgroundService>();
builder.Services.AddSingleton<IDistributedLock, RedisDistributedLock>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Temperature Service API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token only, without 'Bearer ' prefix."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // In development, we want to show detailed error pages and enable Swagger UI.


}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}