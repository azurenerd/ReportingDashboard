namespace AgentSquad.Runner.Services;

public interface IDataCache
{
    Task<T> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
}