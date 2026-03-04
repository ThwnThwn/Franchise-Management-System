namespace ProductService.Application.Interfaces;

public interface IDistributedLock
{
    /// <summary>
    /// Thử chiếm Lock.
    /// ttl: Thời gian tối đa giữ lock (để tránh kẹt lock nếu server sập).
    /// </summary>
    Task<IAsyncDisposable?> TryAcquireLockAsync(string lockName, TimeSpan ttl);
}