using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// In-memory cache provider implementation wrapping IMemoryCache with TTL support and logging.
    /// </summary>
    public class MemoryCacheProvider : IDataCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheProvider> _logger;
        private const string DEFAULT_TTL_HOURS = "1";

        /// <summary>
        /// Initializes a new instance of the MemoryCacheProvider class.
        /// </summary>
        /// <param name="memoryCache">The ASP.NET Core memory cache instance.</param>
        /// <param name="logger">The logger instance.</param>
        public MemoryCacheProvider(IMemoryCache memoryCache, ILogger<MemoryCacheProvider> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a cached value by key asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached value if found and not expired, otherwise null.</returns>
        public Task<T> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Cache Get called with null or empty key.");
                return Task.FromResult<T>(null);
            }

            if (_memoryCache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", key);
                return Task.FromResult(cachedValue);
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", key);
            return Task.FromResult<T>(null);
        }

        /// <summary>
        /// Stores a value in cache with optional expiration asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="expiration">Optional cache expiration duration. Defaults to 1 hour if null.</param>
        /// <returns>A completed task.</returns>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Cache Set called with null or empty key.");
                return Task.CompletedTask;
            }

            if (value == null)
            {
                _logger.LogWarning("Cache Set called with null value for key: {CacheKey}", key);
                return Task.CompletedTask;
            }

            var ttl = expiration ?? TimeSpan.FromHours(1);
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            _memoryCache.Set(key, value, cacheOptions);
            _logger.LogDebug("Cache set for key: {CacheKey} with TTL: {TTL} hours", key, ttl.TotalHours);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes a cached value by key.
        /// </summary>
        /// <param name="key">The cache key to remove.</param>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Cache Remove called with null or empty key.");
                return;
            }

            _memoryCache.Remove(key);
            _logger.LogDebug("Cache entry removed for key: {CacheKey}", key);
        }
    }
}