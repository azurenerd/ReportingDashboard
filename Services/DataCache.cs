using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services;

public class DataCache : IDataCache
{
    private readonly IMemoryCache _memoryCache;

    public DataCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public Task<T> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        }

        var value = _memoryCache.TryGetValue(key, out T cachedValue) ? cachedValue : null;
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Cache value cannot be null");
        }

        var cacheOptions = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        else
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        }

        _memoryCache.Set(key, value, cacheOptions);
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        }

        _memoryCache.Remove(key);
    }
}