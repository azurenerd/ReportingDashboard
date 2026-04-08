namespace AgentSquad.Runner.Services;

/// <summary>
/// Generic in-memory caching service interface.
/// </summary>
public interface IDataCache
{
    /// <summary>
    /// Asynchronously retrieves a cached value by key.
    /// </summary>
    /// <typeparam name="T">Type of cached value (must be a reference type).</typeparam>
    /// <param name="key">Cache key.</param>
    /// <returns>Cached value or null if not found or expired.</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Asynchronously stores a value in cache with optional expiration.
    /// </summary>
    /// <typeparam name="T">Type of value to cache.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="expiration">Optional TTL; defaults to 1 hour if not specified.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Removes a value from cache by key.
    /// </summary>
    /// <param name="key">Cache key to remove.</param>
    void Remove(string key);
}