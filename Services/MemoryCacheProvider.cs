using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services;

/// <summary>
/// In-memory cache implementation wrapping IMemoryCache with async operations and configurable TTL.
/// </summary>
public class MemoryCacheProvider : IDataCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheProvider> _logger;
    private const int DefaultTtlHours = 1;

    public MemoryCacheProvider(IMemoryCache memoryCache, ILogger<MemoryCacheProvider> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a cached value; logs cache hit or miss at Debug level.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("GetAsync called with null or empty key");
            return null;
        }

        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return await Task.FromResult(cachedValue);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return null;
    }

    /// <summary>
    /// Stores a value in cache with specified or default TTL.
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("SetAsync called with null or empty key");
            return;
        }

        if (value == null)
        {
            _logger.LogWarning("SetAsync called with null value for key: {Key}", key);
            return;
        }

        var ttl = expiration ?? TimeSpan.FromHours(DefaultTtlHours);
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(ttl);

        _memoryCache.Set(key, value, cacheOptions);
        _logger.LogDebug("Cache set for key: {Key} with TTL: {Ttl}ms", key, ttl.TotalMilliseconds);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes a cached value by key.
    /// </summary>
    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Remove called with null or empty key");
            return;
        }

        _memoryCache.Remove(key);
        _logger.LogDebug("Cache invalidated for key: {Key}", key);
    }
}