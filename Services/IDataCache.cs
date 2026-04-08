namespace AgentSquad.Runner.Services;

/// <summary>
/// Abstraction for in-memory caching with async operations and TTL support.
/// </summary>
public interface IDataCache
{
    /// <summary>
    /// Retrieves a cached value by key.
    /// </summary>
    /// <typeparam name="T">Type of cached value (must be a class).</typeparam>
    /// <param name="key">Cache key.</param>
    /// <returns>Cached value if present and not expired; null otherwise.</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Stores a value in the cache with optional expiration.
    /// </summary>
    /// <typeparam name="T">Type of value to cache (must be a class).</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="expiration">Optional TTL; defaults to 1 hour if not specified.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Removes a cached value by key.
    /// </summary>
    /// <param name="key">Cache key.</param>
    void Remove(string key);
}