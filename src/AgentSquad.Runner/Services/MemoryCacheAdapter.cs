using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services;

public class MemoryCacheAdapter : IDataCache
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheAdapter(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T> GetAsync<T>(string key) where T : class
    {
        _memoryCache.TryGetValue(key, out T value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var cacheOptions = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
        else
            cacheOptions.SlidingExpiration = TimeSpan.FromMinutes(30);

        _memoryCache.Set(key, value, cacheOptions);
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }
}