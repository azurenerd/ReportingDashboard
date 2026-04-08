namespace AgentSquad.Runner.Services
{
    public class DataCache : IDataCache
    {
        private readonly Dictionary<string, object> _cache = new();
        private readonly object _lock = new();

        public Task<T> GetAsync<T>(string key) where T : class
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var value))
                {
                    return Task.FromResult(value as T);
                }
                return Task.FromResult<T>(null);
            }
        }

        public Task SetAsync<T>(string key, T value) where T : class
        {
            lock (_lock)
            {
                _cache[key] = value;
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            lock (_lock)
            {
                _cache.Remove(key);
                return Task.CompletedTask;
            }
        }
    }
}