namespace AgentSquad.Runner.Interfaces;

/// <summary>
/// Service contract for monitoring data.json file for changes and emitting refresh events.
/// 
/// Responsibility: Monitor data.json file for changes. Debounce rapid file writes. 
/// Emit events to trigger UI refresh. Pure file monitoring, no data parsing or validation.
/// 
/// Lifetime: Singleton (one monitor per app lifetime, shared across all Blazor Server requests)
/// Thread Safety: All events fired on main Blazor Server thread (thread-safe)
/// </summary>
public interface IDataWatcherService : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Event fired asynchronously when file change detected (after debounce period).
    /// 
    /// Handler signature: Func&lt;Task&gt; (async void method, fires and forgets)
    /// Fired on: Main Blazor Server thread (safe for component state updates)
    /// Timing: After 500ms debounce period with no additional file changes
    /// 
    /// Subscribers (typically Index.razor component):
    /// - Reload JSON data via DataLoaderService.LoadAsync()
    /// - Validate via DataValidator.Validate()
    /// - Update component state and call StateHasChanged()
    /// 
    /// Example subscriber:
    /// <code>
    /// _dataWatcherService.OnDataChanged += async () =>
    /// {
    ///     _isLoading = true;
    ///     var report = await _dataLoaderService.LoadAsync();
    ///     _validationResult = _validator.Validate(report);
    ///     StateHasChanged();
    /// };
    /// </code>
    /// </summary>
    event Func<Task> OnDataChanged;

    /// <summary>
    /// Initialize and start file monitoring for the specified data.json path.
    /// </summary>
    /// <param name="dataPath">
    /// Optional path to the file to monitor. If null, defaults to configured DataPath.
    /// Can be relative (e.g., "data.json") or absolute (e.g., "C:\data\project.json").
    /// </param>
    /// <remarks>
    /// - Creates System.IO.FileSystemWatcher on the directory containing dataPath
    /// - Subscribes to Changed event (Last-Write timestamp change)
    /// - Implements 500ms debounce timer (prevents cascade from multiple write events)
    /// - Updates LastRefreshTime on successful change detection
    /// - Does NOT throw exceptions; logs errors internally and continues
    /// - Graceful degradation: If FSW fails to initialize, logs WARNING and app continues
    /// - No retry logic (FSW self-recovers on next file write)
    /// - Call Stop() before calling Start() again to avoid duplicate watchers
    /// </remarks>
    void Start(string dataPath = null);

    /// <summary>
    /// Stop file monitoring and clean up FileSystemWatcher resources.
    /// </summary>
    /// <remarks>
    /// - Disposes FileSystemWatcher instance
    /// - Cancels any pending debounce timers
    /// - Prevents future OnDataChanged events
    /// - Safe to call multiple times (idempotent)
    /// </remarks>
    void Stop();

    /// <summary>
    /// Get the timestamp of the last successful file change detection and refresh.
    /// </summary>
    /// <value>
    /// DateTime (UTC) of the most recent OnDataChanged event fired.
    /// DateTime.MinValue if watcher has not detected any changes yet.
    /// </value>
    /// <remarks>
    /// Updated each time OnDataChanged fires (after debounce).
    /// Useful for displaying "Data refreshed at HH:mm:ss" in UI.
    /// </remarks>
    DateTime LastRefreshTime { get; }

    /// <summary>
    /// Get the last refresh timestamp formatted for UI display (HH:mm:ss).
    /// </summary>
    /// <value>
    /// String in format "HH:mm:ss" (e.g., "14:32:45") in local time.
    /// Empty string if watcher has not detected any changes yet.
    /// </value>
    /// <remarks>
    /// Convenience property that formats LastRefreshTime for display.
    /// Already converted to local time for UI display.
    /// </remarks>
    string LastRefreshTimeFormatted { get; }
}