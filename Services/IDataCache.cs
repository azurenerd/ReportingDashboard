using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public interface IDataCache
    {
        void Set<T>(string key, T value);
        T Get<T>(string key);
        bool TryGetValue<T>(string key, out T value);
        void Remove(string key);
    }
}