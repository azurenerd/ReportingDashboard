using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services;

public interface IDataCache
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    void Remove(string key);
}

public class DataCache : IDataCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DataCache> _logger;

    public DataCache(IMemoryCache memoryCache, ILogger<DataCache> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_memoryCache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var cacheOptions = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        _memoryCache.Set(key, value, cacheOptions);
        _logger.LogDebug("Cached key: {Key} with expiration: {Expiration}", key,
            expiration?.TotalMinutes ?? -1);

        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug("Removed cache entry for key: {Key}", key);
    }
}