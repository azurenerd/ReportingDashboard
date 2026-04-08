using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services;

public class DataCache : IDataCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DataCache> _logger;
    private const string CacheKeyPrefix = "agentsquad_";

    public DataCache(IMemoryCache memoryCache, ILogger<DataCache> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";

        if (_memoryCache.TryGetValue(cacheKey, out T cachedValue))
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", key);
            return cachedValue;
        }

        _logger.LogInformation("Cache miss for key: {CacheKey}, creating value", key);
        var value = factory();

        var cacheEntryOptions = new MemoryCacheEntryOptions();

        if (ttl.HasValue)
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = ttl.Value;
            _logger.LogInformation("Setting cache TTL for key: {CacheKey} to {Seconds} seconds", 
                key, ttl.Value.TotalSeconds);
        }
        else
        {
            cacheEntryOptions.SlidingExpiration = TimeSpan.FromHours(1);
        }

        _memoryCache.Set(cacheKey, value, cacheEntryOptions);
        return value;
    }

    public void Remove(string key)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";
        _memoryCache.Remove(cacheKey);
        _logger.LogInformation("Cache invalidated for key: {CacheKey}", key);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";
        return _memoryCache.TryGetValue(cacheKey, out value);
    }
}