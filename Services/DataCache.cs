using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services;

/// <summary>
/// In-memory cache implementation wrapping IMemoryCache.
/// </summary>
public class DataCache : IDataCache
{
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the DataCache class.
    /// </summary>
    public DataCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    public T Get<T>(string key) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        _memoryCache.TryGetValue(key, out T value);
        return value;
    }

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value), "Cannot cache null values");

        var cacheOptions = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        _memoryCache.Set(key, value, cacheOptions);
    }

    /// <summary>
    /// Removes a value from the cache by key.
    /// </summary>
    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        _memoryCache.Remove(key);
    }
}