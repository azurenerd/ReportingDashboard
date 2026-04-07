using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services;

public class MemoryCacheAdapter : IDataCache
{
    private readonly IMemoryCache _cache;

    public MemoryCacheAdapter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T> GetAsync<T>(string key) where T : class
    {
        _cache.TryGetValue(key, out T value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration;

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public void Remove(string key) => _cache.Remove(key);
}