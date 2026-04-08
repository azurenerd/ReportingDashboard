namespace AgentSquad.Runner.Services;

public interface IDataCache
{
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null);
    void Remove(string key);
    bool TryGetValue<T>(string key, out T value);
}