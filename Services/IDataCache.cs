using System;
using System.Threading.Tasks;

namespace AgentSquad.Services
{
    /// <summary>
    /// Service interface for in-memory caching layer supporting async operations.
    /// </summary>
    public interface IDataCache
    {
        /// <summary>
        /// Retrieves a cached value by key asynchronously.
        /// </summary>
        /// <typeparam name="T">Type of cached value (must be reference type)</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached value or null if key not found or expired</returns>
        Task<T> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Stores a value in cache with optional time-to-live asynchronously.
        /// </summary>
        /// <typeparam name="T">Type of value to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="expiration">Optional TTL; if null, cache indefinitely</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Removes a cached value by key.
        /// </summary>
        /// <param name="key">Cache key to remove</param>
        void Remove(string key);
    }
}