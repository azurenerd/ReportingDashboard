namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Abstraction for in-memory caching operations with TTL support.
    /// </summary>
    public interface IDataCache
    {
        /// <summary>
        /// Retrieves a cached value by key asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached value if found and not expired, otherwise null.</returns>
        Task<T> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Stores a value in cache with optional expiration asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="expiration">Optional cache expiration duration. Defaults to 1 hour if null.</param>
        /// <returns>A completed task.</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Removes a cached value by key.
        /// </summary>
        /// <param name="key">The cache key to remove.</param>
        void Remove(string key);
    }
}