using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DataService : IDisposable
{
    private const int ExpectedSchemaVersion = 1;
    private const int DebounceMs = 500;

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

        LoadData();

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
            if (_currentData?.NowDateOverride is { } overrideStr
                && !string.IsNullOrWhiteSpace(overrideStr))
            {
                try
                {
                    return DateOnly.ParseExact(overrideStr, "yyyy-MM-dd");
                }
                catch
                {
                    // Invalid override format — fall through to system date
                }
            }
            return DateOnly.FromDateTime(DateTime.Today);
        }
    }

    /// <summary>
    /// Returns the abbreviated month name (e.g. "Apr") used for current-month heatmap highlighting.
    /// Uses currentMonthOverride from data.json if present; otherwise derives from the effective date.
    /// </summary>
    public string GetCurrentMonthName()
    {
        lock (_lock)
        {
            if (_currentData?.CurrentMonthOverride is { } overrideMonth
                && !string.IsNullOrWhiteSpace(overrideMonth))
            {
                return overrideMonth;
            }
            return GetEffectiveDateInternal().ToString("MMM");
        }
    }

    private DateOnly GetEffectiveDateInternal()
    {
        // Must be called under lock
        if (_currentData?.NowDateOverride is { } overrideStr
            && !string.IsNullOrWhiteSpace(overrideStr))
        {
            try
            {
                return DateOnly.ParseExact(overrideStr, "yyyy-MM-dd");
            }
            catch
            {
                // Fall through
            }
        }
        return DateOnly.FromDateTime(DateTime.Today);
    }

    private void LoadData()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                lock (_lock)
                {
                    // Only clear data if we have nothing yet (preserve last-known-good on reload)
                    if (_currentData is null)
                        _currentData = null;
                    _currentError = "No data.json found. Place a valid data.json file in the wwwroot/ directory.";
                }
                return;
            }

            var json = File.ReadAllText(_dataFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);

            if (data is null)
            {
                lock (_lock)
                {
                    _currentError = "data.json contains invalid JSON: deserialization returned null.";
                    // Preserve last-known-good data on reload
                }
                return;
            }

            if (data.SchemaVersion != ExpectedSchemaVersion)
            {
                lock (_lock)
                {
                    _currentError = $"data.json schemaVersion is {data.SchemaVersion}, expected {ExpectedSchemaVersion}. Update your data.json to match the current schema.";
                    // Don't update _currentData on schema mismatch — preserve last-known-good
                }
                return;
            }

            lock (_lock)
            {
                _currentData = data;
                _currentError = null;
            }
        }
        catch (JsonException ex)
        {
            lock (_lock)
            {
                _currentError = $"data.json contains invalid JSON: {ex.Message}";
                // Preserve last-known-good _currentData on reload failure
            }
        }
        catch (IOException ex)
        {
            lock (_lock)
            {
                _currentError = $"Error reading data.json: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
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

    private void OnDebounceElapsed(object? state)
    {
        Console.WriteLine($"[DataService] Reloading data.json at {DateTime.Now:HH:mm:ss.fff}");
        LoadData();
        OnDataChanged?.Invoke();
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _debounceTimer.Dispose();
    }
}