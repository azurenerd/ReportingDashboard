using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Monitors data.json file for changes and emits OnDataChanged event after 500ms debounce.
/// Fires event on main Blazor Server thread for thread-safe UI updates.
/// </summary>
public class DataWatcherService : IDataWatcherService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataWatcherService> _logger;
    
    private FileSystemWatcher? _fileSystemWatcher;
    private Timer? _debounceTimer;
    private DateTime _lastRefreshTime = DateTime.MinValue;
    private string _lastWatchedPath = string.Empty;
    private bool _disposed;
    
    public event Func<Task>? OnDataChanged;

    public DataWatcherService(IConfiguration configuration, ILogger<DataWatcherService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the timestamp of the last successful data refresh.
    /// </summary>
    public DateTime LastRefreshTime => _lastRefreshTime;

    /// <summary>
    /// Gets the formatted timestamp for UI display (HH:mm:ss).
    /// Returns "Not loaded" if no refresh has occurred yet.
    /// </summary>
    public string LastRefreshTimeFormatted
    {
        get
        {
            if (_lastRefreshTime == DateTime.MinValue)
                return "Not loaded";
            return _lastRefreshTime.ToString("HH:mm:ss");
        }
    }

    /// <summary>
    /// Initialize and start file watcher for specified data path.
    /// Uses configuration value if dataPath is null.
    /// </summary>
    /// <param name="dataPath">Optional file path; defaults to AppSettings:DataPath from configuration</param>
    public void Start(string? dataPath = null)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot start DataWatcherService after it has been disposed");
            return;
        }

        try
        {
            // Resolve data path: parameter > config > default
            string resolvedPath = ResolveDataPath(dataPath);
            
            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                _logger.LogError("DataPath is empty or null; cannot start file watcher");
                return;
            }

            // Validate file exists
            if (!File.Exists(resolvedPath))
            {
                _logger.LogError($"Data file not found at path: {resolvedPath}");
                return;
            }

            _lastWatchedPath = resolvedPath;
            
            // Initialize FileSystemWatcher
            string? directoryPath = Path.GetDirectoryName(resolvedPath);
            string fileName = Path.GetFileName(resolvedPath);
            
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                _logger.LogError($"Cannot extract directory from path: {resolvedPath}");
                return;
            }

            _fileSystemWatcher = new FileSystemWatcher(directoryPath, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _fileSystemWatcher.Changed += OnFileChanged;
            _fileSystemWatcher.Error += OnFileWatcherError;

            _logger.LogInformation($"DataWatcher started monitoring: {resolvedPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to start DataWatcher: {ex.Message}");
            // Graceful degradation: log error but don't throw
        }
    }

    /// <summary>
    /// Stop file watcher and clean up resources.
    /// </summary>
    public void Stop()
    {
        try
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Changed -= OnFileChanged;
                _fileSystemWatcher.Error -= OnFileWatcherError;
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }

            _debounceTimer?.Dispose();
            _debounceTimer = null;

            _logger.LogInformation("DataWatcher stopped");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error stopping DataWatcher: {ex.Message}");
        }
    }

    /// <summary>
    /// Dispose resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _disposed = true;
    }

    /// <summary>
    /// Async dispose.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Dispose();
        await Task.CompletedTask;
    }

    /// <summary>
    /// FileSystemWatcher Changed event handler.
    /// Implements debounce logic using timer.
    /// </summary>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            _logger.LogInformation($"File change detected at {DateTime.Now:HH:mm:ss.fff}: {e.Name}");

            // Cancel existing debounce timer if running
            _debounceTimer?.Dispose();

            // Create new debounce timer (500ms)
            int debounceMs = GetDebounceInterval();
            _debounceTimer = new Timer(
                async _ => await OnDebounceElapsed(),
                null,
                debounceMs,
                Timeout.Infinite
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error in file change handler: {ex.Message}");
        }
    }

    /// <summary>
    /// Called when debounce timer elapses (500ms without additional file changes).
    /// Fires OnDataChanged event on main Blazor thread.
    /// </summary>
    private async Task OnDebounceElapsed()
    {
        try
        {
            _logger.LogInformation($"Debounce elapsed, triggering OnDataChanged at {DateTime.Now:HH:mm:ss}");

            // Update refresh timestamp
            _lastRefreshTime = DateTime.Now;

            // Fire OnDataChanged event if subscribed
            if (OnDataChanged != null)
            {
                // Invoke handlers - each handler is Func<Task>
                var delegates = OnDataChanged.GetInvocationList();
                foreach (Func<Task> handler in delegates)
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in OnDataChanged handler: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in debounce elapsed handler: {ex.Message}");
        }
    }

    /// <summary>
    /// FileSystemWatcher error event handler.
    /// Gracefully handles FSW errors without crashing.
    /// </summary>
    private void OnFileWatcherError(object sender, ErrorEventArgs e)
    {
        Exception? ex = e.GetException();
        if (ex != null)
        {
            _logger.LogWarning($"FileSystemWatcher error: {ex.Message}");
        }
        else
        {
            _logger.LogWarning("FileSystemWatcher encountered an unknown error");
        }
    }

    /// <summary>
    /// Resolve data path from parameter, config, or default.
    /// </summary>
    private string ResolveDataPath(string? parameterPath)
    {
        // If parameter provided, use it
        if (!string.IsNullOrWhiteSpace(parameterPath))
        {
            return parameterPath;
        }

        // Try configuration
        string? configPath = _configuration["AppSettings:DataPath"];
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            return configPath;
        }

        // Default fallback
        return "data.json";
    }

    /// <summary>
    /// Get debounce interval from configuration or default to 500ms.
    /// </summary>
    private int GetDebounceInterval()
    {
        string? debounceStr = _configuration["AppSettings:DebounceIntervalMs"];
        if (int.TryParse(debounceStr, out int debounceMs) && debounceMs > 0)
        {
            return debounceMs;
        }

        return 500; // Default 500ms
    }
}