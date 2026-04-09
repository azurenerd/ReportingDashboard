using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Interfaces
{
    public interface IDataWatcherService : IDisposable, IAsyncDisposable
    {
        event Func<Task> OnDataChanged;

        void Start(string dataPath = null);

        void Stop();

        DateTime LastRefreshTime { get; }

        string LastRefreshTimeFormatted { get; }
    }
}