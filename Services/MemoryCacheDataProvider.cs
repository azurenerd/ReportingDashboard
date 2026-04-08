using Microsoft.Extensions.Caching.Memory;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class MemoryCacheDataProvider : IDataCache
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheDataProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set<T>(string key, T value)
        {
            _memoryCache.Set(key, value, TimeSpan.FromHours(1));
        }

        public T Get<T>(string key)
        {
            return _memoryCache.TryGetValue(key, out T value) ? value : default;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}