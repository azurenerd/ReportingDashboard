namespace AgentSquad.Runner.Services;

using Microsoft.Extensions.Caching.Memory;

public class DataCache : IDataCache
{
    private readonly IMemoryCache _memoryCache;

    public DataCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        return await Task.FromResult(_memoryCache.TryGetValue(key, out T? value) ? value : null);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var cacheOptions = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        _memoryCache.Set(key, value, cacheOptions);
        await Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }
}