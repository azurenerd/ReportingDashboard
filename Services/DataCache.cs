using Microsoft.Extensions.Caching.Memory;

namespace AgentSquad.Runner.Services
{
    public class DataCache : IDataCache
    {
        private readonly IMemoryCache _memoryCache;

        public DataCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            return await Task.FromResult(_memoryCache.TryGetValue(key, out T value) ? value : null);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
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
            await Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}