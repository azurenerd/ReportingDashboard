namespace AgentSquad.Runner.Interfaces;

/// <summary>
/// Interface for monitoring data.json file changes with debounced event emission.
/// </summary>
public interface IDataWatcherService : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Event fired asynchronously after debounce period when file change detected.
    /// Handler signature: Func&lt;Task&gt; (async event handler).
    /// Fired on: Main Blazor Server thread (thread-safe for UI updates).
    /// </summary>
    event Func<Task>? OnDataChanged;

    /// <summary>
    /// Initialize and start file watcher for specified data path.
    /// Does not throw exceptions; logs errors internally for graceful degradation.
    /// </summary>
    /// <param name="dataPath">Optional path to monitor (e.g., "./data.json").
    /// Defaults to AppSettings:DataPath from configuration if null.</param>
    void Start(string? dataPath = null);

    /// <summary>
    /// Stop file watcher and clean up resources.
    /// Safe to call multiple times.
    /// </summary>
    void Stop();

    /// <summary>
    /// Get timestamp of last successful data refresh.
    /// Returns DateTime.MinValue if no refresh has occurred.
    /// </summary>
    DateTime LastRefreshTime { get; }

    /// <summary>
    /// Get formatted timestamp for UI display in HH:mm:ss format.
    /// Returns "Not loaded" if no refresh has occurred.
    /// </summary>
    string LastRefreshTimeFormatted { get; }
}