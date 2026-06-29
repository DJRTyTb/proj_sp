namespace Server.Services.Infrastructure;

public class GameLockProvider
{
    private readonly Dictionary<Guid, SemaphoreSlim> _locks = [];

    public SemaphoreSlim GetLock(Guid gameId)
    {
        lock (_locks)
        {
            if (!_locks.TryGetValue(gameId, out SemaphoreSlim? semaphore))
            {
                semaphore = new SemaphoreSlim(1, 1);
                _locks[gameId] = semaphore;
            }

            return semaphore;
        }
    }

    public void RemoveLock(Guid gameId)
    {
        lock (_locks)
        {
            if (!_locks.Remove(gameId, out SemaphoreSlim? semaphore))
                return;

            semaphore.Dispose();
        }
    }
}