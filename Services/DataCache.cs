namespace AgentSquad.Runner.Services;

using Microsoft.Extensions.Caching.Memory;

public class DataCache : IDataCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DataCache> _logger;

    public DataCache(IMemoryCache memoryCache, ILogger<DataCache> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<T> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        }

        var result = _memoryCache.TryGetValue(key, out T value) ? value : null;
        
        if (result != null)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
        }

        return Task.FromResult(result);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
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
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
        }

        _memoryCache.Set(key, value, cacheOptions);
        _logger.LogDebug("Cache set for key: {Key}", key);

        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        }

        _memoryCache.Remove(key);
        _logger.LogDebug("Cache removed for key: {Key}", key);
    }
}