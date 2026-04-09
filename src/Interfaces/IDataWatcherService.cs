using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Interfaces
{
    public interface IDataWatcherService : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Event fired asynchronously after debounce period when file change detected.
        /// Handler: Func&lt;Task&gt; (async event)
        /// Fired on: Main Blazor Server thread (thread-safe)
        /// </summary>
        event Func<Task> OnDataChanged;

        /// <summary>
        /// Initialize file watcher for specified data path.
        /// </summary>
        /// <param name="dataPath">Path to monitor (e.g., "./data.json")</param>
        /// <remarks>Does not throw; logs errors internally. Graceful degradation if FSW fails.</remarks>
        void Start(string dataPath = null);

        /// <summary>
        /// Stop file watcher and clean up resources.
        /// </summary>
        void Stop();

        /// <summary>
        /// Get timestamp of last successful refresh.
        /// </summary>
        DateTime LastRefreshTime { get; }

        /// <summary>
        /// Get formatted timestamp for UI display (HH:mm:ss).
        /// </summary>
        string LastRefreshTimeFormatted { get; }
    }
}