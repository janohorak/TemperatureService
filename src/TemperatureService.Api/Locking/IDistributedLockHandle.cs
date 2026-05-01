namespace TemperatureService.Api.Locking;

public interface IDistributedLockHandle : IAsyncDisposable
{
    string Key { get; }
}