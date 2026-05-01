namespace TemperatureService.Api.Locking;

public interface IDistributedLock
{
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken);
}