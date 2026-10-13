using System.Collections.Concurrent;

namespace ServiceDeskSystem.Application.Common;

/// <summary>
/// One async lock per key: work on the same key runs one at a time, work on different keys in parallel.
/// Covers this process only.
/// </summary>
internal sealed class KeyedLock<TKey>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> locks = new ();

    public async Task<IDisposable> AcquireAsync(TKey key)
    {
        var semaphore = this.locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync().ConfigureAwait(false);
        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.released, 1) == 0)
            {
                semaphore.Release();
            }
        }
    }
}
