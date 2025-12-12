namespace spinner;

public class OwnedSemaphore : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly HashSet<object> _owners;
    private readonly string _name;

    public OwnedSemaphore(int initialCount, int maxCount, string name = "default")
    {
        _semaphore = new SemaphoreSlim(initialCount, maxCount);
        _owners = new HashSet<object>();
        _name = name;
    }

    public async Task WaitAsync(int randomId)
    {
        await _semaphore.WaitAsync();
        lock (_owners)
        {
            _owners.Add(randomId);
        }
    }

    public void Release(int randomId)
    {
        lock (_owners)
        {
            if (!_owners.Contains(randomId))
            {
                return;
            }
            _semaphore.Release();
            _owners.Remove(randomId);
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
