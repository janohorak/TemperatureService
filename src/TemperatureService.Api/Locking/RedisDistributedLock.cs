using StackExchange.Redis;
using TemperatureService.Api.Infrastructure;
using TemperatureService.Api.Infrastructure.Logging;

namespace TemperatureService.Api.Locking;

public sealed class RedisDistributedLock : IDistributedLock
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisDistributedLock> _logger;

    public RedisDistributedLock(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisDistributedLock> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<IDistributedLockHandle?> TryAcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var token = Guid.NewGuid().ToString("N");

        var acquired = await database.StringSetAsync(
            key,
            token,
            expiry,
            When.NotExists);

        if (!acquired)
        {
            _logger.LogInformation(
                TemperatureLogEvents.DistributedLockNotAcquired,
                "Pod {PodName}: distributed lock {LockKey} was not acquired.",
                RuntimeInfo.PodName,
                key);

            return null;
        }

        _logger.LogInformation(
            TemperatureLogEvents.DistributedLockAcquired,
            "Pod {PodName}: distributed lock {LockKey} acquired.",
            RuntimeInfo.PodName,
            key);

        return new RedisDistributedLockHandle(
            database,
            key,
            token,
            _logger);
    }
}