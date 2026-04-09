using System.IO;
using System.Timers;
using AgentSquad.Runner.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Monitors data.json file for changes using FileSystemWatcher with 500ms debounce.
/// Emits OnDataChanged async event on main Blazor Server thread when file changes detected.
/// Tracks LastRefreshTime timestamp for UI display.
/// 
/// Thread Safety: OnDataChanged event is fired on the main Blazor Server thread via
/// implicit marshaling through the SynchronizationContext. Blazor components subscribing
/// to this event will have their handlers invoked on the UI thread automatically.
/// </summary>
public class DataWatcherService : IDataWatcherService, IDisposable, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataWatcherService> _logger;
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private DateTime _lastRefreshTime;
    private readonly SynchronizationContext? _syncContext;

    /// <summary>
    /// Event fired asynchronously after debounce period when file change detected.
    /// Handler: Func<Task> (async event)
    /// Fired on: Main Blazor Server thread (via SynchronizationContext marshaling)
    /// </summary>
    public event Func<Task>? OnDataChanged;

    /// <summary>
    /// Get timestamp of last successful refresh.
    /// </summary>
    public DateTime LastRefreshTime => _lastRefreshTime;

    /// <summary>
    /// Get formatted timestamp for UI display (HH:mm:ss).
    /// </summary>
    public string LastRefreshTimeFormatted => _lastRefreshTime.ToString("HH:mm:ss");

    /// <summary>
    /// Initialize DataWatcherService with configuration and logging.
    /// Captures current SynchronizationContext for thread-safe event firing.
    /// </summary>
    public DataWatcherService(IConfiguration configuration, ILogger<DataWatcherService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _lastRefreshTime = DateTime.Now;
        _syncContext = SynchronizationContext.Current;
        _logger.LogInformation("DataWatcherService instantiated");
    }

    /// <summary>
    /// Initialize file watcher for specified data path.
    /// </summary>
    /// <param name="dataPath">Path to monitor (e.g., "./data.json"). Defaults to config or "./data.json"</param>
    /// <remarks>Does not throw; logs errors internally. Graceful degradation if FSW fails.</remarks>
    public void Start(string? dataPath = null)
    {
        try
        {
            // Resolve dataPath: parameter > config > default
            dataPath ??= _configuration.GetValue<string>("AppSettings:DataPath") ?? "./data.json";

            var fullPath = Path.GetFullPath(dataPath);
            var directory = Path.GetDirectoryName(fullPath) ?? ".";
            var filename = Path.GetFileName(fullPath);

            _logger.LogInformation($"Starting DataWatcher for: {dataPath}");

            // Create and configure FileSystemWatcher
            _watcher = new FileSystemWatcher(directory)
            {
                Filter = filename,
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = false
            };

            // Subscribe to Changed event
            _watcher.Changed += OnFileChanged;

            // Enable raising events
            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation($"DataWatcher started successfully for: {dataPath}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to start DataWatcher: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop file watcher and cancel debounce timer.
    /// </summary>
    public void Stop()
    {
        _logger.LogInformation("Stopping DataWatcher");

        try
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
            }

            if (_debounceTimer != null)
            {
                _debounceTimer.Stop();
                _debounceTimer.Dispose();
                _debounceTimer = null;
            }

            _logger.LogInformation("DataWatcher stopped");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error stopping DataWatcher: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle file change events with debounce logic.
    /// </summary>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File changed detected");

        try
        {
            if (_debounceTimer?.Enabled == true)
            {
                _debounceTimer.Stop();
            }

            var debounceMs = _configuration.GetValue("AppSettings:DebounceIntervalMs", 500);

            _debounceTimer = new Timer(debounceMs)
            {
                AutoReset = false,
                Enabled = false
            };

            _debounceTimer.Elapsed += async (s, timerEventArgs) => await OnDebounceTimerElapsed();

            _debounceTimer.Start();

            _logger.LogDebug($"Debounce timer started ({debounceMs}ms)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error in OnFileChanged: {ex.Message}");
        }
    }

    /// <summary>
    /// Fire OnDataChanged event after debounce period completes.
    /// </summary>
    private async Task OnDebounceTimerElapsed()
    {
        try
        {
            _lastRefreshTime = DateTime.Now;
            _logger.LogInformation($"Data refresh triggered at {_lastRefreshTime:HH:mm:ss}");

            if (OnDataChanged != null)
            {
                if (_syncContext != null)
                {
                    await _syncContext.InvokeAsync(async () => await OnDataChanged.Invoke());
                }
                else
                {
                    await OnDataChanged.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in OnDebounceTimerElapsed: {ex.Message}");
        }
    }

    /// <summary>
    /// Dispose managed resources.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _watcher?.Dispose();
        _watcher = null;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously dispose managed resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Dispose();
        await ValueTask.CompletedTask;
    }
}