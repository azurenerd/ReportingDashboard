using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services;

/// <summary>
/// In-memory caching service implementation wrapping IMemoryCache.
/// Provides typed, async-friendly cache operations with configurable TTL.
/// </summary>
public class DataCache : IDataCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DataCache> _logger;
    private const int DefaultTtlMinutes = 60;

    /// <summary>
    /// Initializes a new instance of DataCache.
    /// </summary>
    /// <param name="memoryCache">ASP.NET Core IMemoryCache implementation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public DataCache(IMemoryCache memoryCache, ILogger<DataCache> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Asynchronously retrieves a cached value by key.
    /// Returns null if key not found or has expired.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key cannot be null or whitespace.", nameof(key));
        }

        return await Task.FromResult(_memoryCache.TryGetValue(key, out T? value) ? value : null);
    }

    /// <summary>
    /// Asynchronously stores a value in cache with optional expiration.
    /// If expiration not specified, defaults to 1 hour.
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key cannot be null or whitespace.", nameof(key));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Cache value cannot be null.");
        }

        var ttl = expiration ?? TimeSpan.FromMinutes(DefaultTtlMinutes);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        _memoryCache.Set(key, value, cacheOptions);
        _logger.LogDebug("Cached value with key '{CacheKey}' for {TtlMinutes} minutes.", key, ttl.TotalMinutes);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes a value from cache by key.
    /// No-op if key does not exist.
    /// </summary>
    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key cannot be null or whitespace.", nameof(key));
        }

        _memoryCache.Remove(key);
        _logger.LogDebug("Removed cache entry with key '{CacheKey}'.", key);
    }
}