using StackExchange.Redis;
using TemperatureService.Api.Infrastructure;
using TemperatureService.Api.Infrastructure.Logging;

namespace TemperatureService.Api.Locking;

public sealed class RedisDistributedLockHandle : IDistributedLockHandle
{
    private static readonly LuaScript ReleaseScript = LuaScript.Prepare("""
        if redis.call("get", @key) == @token then
            return redis.call("del", @key)
        else
            return 0
        end
        """);

    private readonly IDatabase _database;
    private readonly string _token;
    private readonly ILogger _logger;
    private bool _disposed;

    public RedisDistributedLockHandle(
        IDatabase database,
        string key,
        string token,
        ILogger logger)
    {
        _database = database;
        Key = key;
        _token = token;
        _logger = logger;
    }

    public string Key { get; }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            var result = await _database.ScriptEvaluateAsync(
                ReleaseScript,
                new
                {
                    key = (RedisKey)Key,
                    token = (RedisValue)_token
                });

            _logger.LogInformation(
                TemperatureLogEvents.DistributedLockReleased,
                "Pod {PodName}: distributed lock {LockKey} released. Result: {Result}.",
                RuntimeInfo.PodName,
                Key,
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                TemperatureLogEvents.DistributedLockReleaseFailed,
                ex,
                "Pod {PodName}: failed to release distributed lock {LockKey}.",
                RuntimeInfo.PodName,
                Key);
        }
    }
}