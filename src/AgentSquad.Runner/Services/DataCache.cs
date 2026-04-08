using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services
{
    public interface IDataCache
    {
        Task<T> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        void Remove(string key);
    }

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

            var value = _memoryCache.TryGetValue(key, out T cachedValue) ? cachedValue : null;
            return Task.FromResult(value);
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
            else
            {
                cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            }

            _memoryCache.Set(key, value, cacheOptions);
            _logger.LogDebug("Cached value for key '{Key}' with expiration {Expiration}", key, expiration?.TotalSeconds ?? 3600);

            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
            }

            _memoryCache.Remove(key);
            _logger.LogDebug("Removed cache entry for key '{Key}'", key);
        }
    }
}