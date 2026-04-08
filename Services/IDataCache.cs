namespace AgentSquad.Runner.Services
{
    public interface IDataCache
    {
        T Get<T>(string key) where T : class;
        void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        void Remove(string key);
        bool Exists(string key);
    }
}