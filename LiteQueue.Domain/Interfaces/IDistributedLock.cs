using LiteQueue.Domain.Models;

namespace LiteQueue.Domain.Interfaces;

public interface IDistributedLock
{
    Task<bool> TryAcquireAsync(string lockKey, TimeSpan lockDuration);
    Task ReleaseAsync(string lockKey);
}
