namespace AgentSquad.Runner.Services;

/// <summary>
/// Service interface for in-memory caching of application data.
/// </summary>
public interface IDataCache
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value, or null if not found or expired.</returns>
    T Get<T>(string key) where T : class;

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Optional expiration time span. If null, cache indefinitely.</param>
    void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Removes a value from the cache by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    void Remove(string key);
}