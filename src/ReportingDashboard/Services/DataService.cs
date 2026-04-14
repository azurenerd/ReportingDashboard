using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

/// <summary>
/// Singleton service that loads, caches, and watches wwwroot/data.json.
/// Implements live-reload via FileSystemWatcher with 500ms debounce.
/// On successful reload, updates cached data and clears error.
/// On failed reload, preserves last-known-good data and sets error message.
/// Fires OnDataChanged after every reload so Blazor components can re-render.
/// </summary>
public class DataService : IDisposable
{
    private const int ExpectedSchemaVersion = 1;
    private const int DebounceMs = 500;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly string _dataFilePath;
    private readonly object _lock = new();
    private readonly FileSystemWatcher? _watcher;
    private readonly Timer _debounceTimer;

    private DashboardData? _currentData;
    private string? _currentError;

    /// <summary>
    /// Fired after every data reload attempt (success or failure).
    /// Subscribers MUST marshal to their own sync context (e.g., InvokeAsync).
    /// </summary>
    public event Action? OnDataChanged;

    public DataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.WebRootPath, "data.json");
        _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);

        // Initial load on construction
        LoadData();

        // Set up FileSystemWatcher for live-reload
        try
        {
            var directory = Path.GetDirectoryName(_dataFilePath)!;
            _watcher = new FileSystemWatcher(directory, "data.json")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DataService] FileSystemWatcher could not be started: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns the current dashboard data, or null if no valid data has been loaded.
    /// </summary>
    public DashboardData? GetData()
    {
        lock (_lock)
        {
            return _currentData;
        }
    }

    /// <summary>
    /// Returns the current error message, or null if the last load was successful.
    /// </summary>
    public string? GetError()
    {
        lock (_lock)
        {
            return _currentError;
        }
    }

    /// <summary>
    /// Returns the effective "now" date used for the timeline NOW line.
    /// Uses nowDateOverride from data.json if present and valid; otherwise uses DateTime.Today.
    /// </summary>
    public DateOnly GetEffectiveDate()
    {
        lock (_lock)
        {
            return GetEffectiveDateInternal();
        }
    }

    /// <summary>
    /// Returns the abbreviated month name (e.g. "Apr") used for current-month heatmap highlighting.
    /// Checks CurrentMonthOverride first (direct month name), then derives from effective date.
    /// </summary>
    public string GetCurrentMonthName()
    {
        lock (_lock)
        {
            if (!string.IsNullOrWhiteSpace(_currentData?.CurrentMonthOverride))
            {
                return _currentData.CurrentMonthOverride;
            }
            return GetEffectiveDateInternal().ToString("MMM");
        }
    }

    // Must be called under lock
    private DateOnly GetEffectiveDateInternal()
    {
        if (_currentData?.NowDateOverride is { } overrideStr
            && !string.IsNullOrWhiteSpace(overrideStr))
        {
            try
            {
                return DateOnly.ParseExact(overrideStr, "yyyy-MM-dd");
            }
            catch
            {
                // Invalid override format - fall through to system date
            }
        }
        return DateOnly.FromDateTime(DateTime.Today);
    }

    /// <summary>
    /// Reads and parses data.json under a single lock acquisition.
    /// On success, replaces cached data and clears error.
    /// On failure, preserves last-known-good data (if any) and sets error message.
    /// </summary>
    private void LoadData()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    // _currentData intentionally unchanged - preserve last-known-good data
                    _currentError = "No data.json found. Place a valid data.json file in the wwwroot/ directory.";
                    return;
                }

                var json = File.ReadAllText(_dataFilePath);
                var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

                if (data is null)
                {
                    // _currentData intentionally unchanged - preserve last-known-good data
                    _currentError = "data.json contains invalid JSON: deserialization returned null.";
                    return;
                }

                if (data.SchemaVersion != ExpectedSchemaVersion)
                {
                    // _currentData intentionally unchanged - preserve last-known-good data
                    _currentError = $"data.json schemaVersion is {data.SchemaVersion}, expected {ExpectedSchemaVersion}. Update your data.json to match the current schema.";
                    return;
                }

                // Success - update cached data and clear error
                _currentData = data;
                _currentError = null;
            }
            catch (JsonException ex)
            {
                // _currentData intentionally unchanged - preserve last-known-good data
                _currentError = $"data.json contains invalid JSON: {ex.Message}";
            }
            catch (IOException ex)
            {
                // _currentData intentionally unchanged - preserve last-known-good data
                _currentError = $"Error reading data.json: {ex.Message}";
            }
            catch (Exception ex)
            {
                // _currentData intentionally unchanged - preserve last-known-good data
                _currentError = $"Error reading data.json: {ex.Message}";
            }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer.Change(DebounceMs, Timeout.Infinite);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _debounceTimer.Change(DebounceMs, Timeout.Infinite);
    }

    /// <summary>
    /// Called after the 500ms debounce period. Reloads data and notifies subscribers.
    /// </summary>
    private void OnDebounceElapsed(object? state)
    {
        Console.WriteLine($"[DataService] data.json changed, reloading...");
        LoadData();

        // Log outcome
        var error = GetError();
        if (error is null)
        {
            Console.WriteLine($"[DataService] data.json reloaded successfully at {DateTime.Now:HH:mm:ss.fff}");
        }
        else
        {
            Console.WriteLine($"[DataService] data.json reload failed: {error}");
        }

        // Invoke each subscriber individually so one throwing doesn't prevent others from firing
        RaiseOnDataChanged();
    }

    /// <summary>
    /// Invokes each OnDataChanged subscriber individually, catching exceptions per-subscriber
    /// so that a failing subscriber does not prevent subsequent subscribers from being notified.
    /// </summary>
    private void RaiseOnDataChanged()
    {
        var handler = OnDataChanged;
        if (handler is null) return;

        foreach (var d in handler.GetInvocationList())
        {
            try
            {
                ((Action)d).Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DataService] OnDataChanged subscriber threw: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _debounceTimer.Dispose();
    }
}