using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Services
{
    /// <summary>
    /// In-memory cache implementation wrapping IMemoryCache.
    /// Provides async-compatible caching for Project data with optional TTL.
    /// </summary>
    public class DataCache : IDataCache
    {
        private readonly IMemoryCache _memoryCache;

        public DataCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        /// <summary>
        /// Retrieves cached value asynchronously. Returns null if not found or expired.
        /// </summary>
        public Task<T> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            _memoryCache.TryGetValue(key, out T cachedValue);
            return Task.FromResult(cachedValue);
        }

        /// <summary>
        /// Stores value in cache with optional TTL.
        /// </summary>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value), "Cache value cannot be null");

            var cacheOptions = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
                cacheOptions.AbsoluteExpirationRelativeToNow = expiration.Value;

            _memoryCache.Set(key, value, cacheOptions);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes value from cache by key.
        /// </summary>
        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            _memoryCache.Remove(key);
        }
    }
}