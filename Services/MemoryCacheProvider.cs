using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services
{
    public class MemoryCacheProvider : IDataCache
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheProvider> _logger;

        public MemoryCacheProvider(IMemoryCache cache, ILogger<MemoryCacheProvider> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T Get<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));
            }

            if (_cache.TryGetValue(key, out T value))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return value;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return null;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Cache value cannot be null.");
            }

            var cacheOptions = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                cacheOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
                _logger.LogDebug("Caching key: {Key} with expiration: {Expiration}", key, expiration.Value.TotalSeconds);
            }
            else
            {
                _logger.LogDebug("Caching key: {Key} without expiration", key);
            }

            _cache.Set(key, value, cacheOptions);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));
            }

            _cache.Remove(key);
            _logger.LogDebug("Removed cache key: {Key}", key);
        }

        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));
            }

            return _cache.TryGetValue(key, out _);
        }
    }
}