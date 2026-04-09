using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Interfaces
{
    /// <summary>
    /// Service for monitoring data.json file changes with debounce support.
    /// Fires OnDataChanged event on main Blazor thread after 500ms debounce period.
    /// Implements IDisposable and IAsyncDisposable for proper resource cleanup.
    /// </summary>
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
        /// Graceful degradation: does not throw; logs errors internally.
        /// If FSW fails, app continues without file monitoring capability.
        /// </summary>
        /// <param name="dataPath">Path to monitor (e.g., "./data.json"). Defaults to AppSettings:DataPath if null.</param>
        void Start(string dataPath = null);

        /// <summary>
        /// Stop file watcher and release resources.
        /// Graceful degradation: does not throw exceptions.
        /// </summary>
        void Stop();

        /// <summary>
        /// Get timestamp of last successful refresh (UTC).
        /// Updated when OnDataChanged event fires.
        /// </summary>
        DateTime LastRefreshTime { get; }

        /// <summary>
        /// Get formatted timestamp for UI display (HH:mm:ss format).
        /// Example: "14:32:45"
        /// </summary>
        string LastRefreshTimeFormatted { get; }
    }
}