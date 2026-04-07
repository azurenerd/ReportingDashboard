namespace AgentSquad.Runner.Services;

public interface IDataCache
{
    Task<T> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    void Remove(string key);
}