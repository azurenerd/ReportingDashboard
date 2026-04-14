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
            Console.WriteLine($"FileSystemWatcher could not be started: {ex.Message}");
        }
    }

    public DashboardData? GetData()
    {
        lock (_lock)
        {
            return _currentData;
        }
    }

    public string? GetError()
    {
        lock (_lock)
        {
            return _currentError;
        }
    }

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
                    // Fall through to default
                }
            }
            return DateOnly.FromDateTime(DateTime.Today);
        }
    }

    private void LoadData()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                lock (_lock)
                {
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
                }
                return;
            }

            if (data.SchemaVersion != ExpectedSchemaVersion)
            {
                lock (_lock)
                {
                    _currentError = $"data.json schemaVersion is {data.SchemaVersion}, expected {ExpectedSchemaVersion}. Update your data.json to match the current schema.";
                    // Don't update _currentData on schema mismatch for initial load
                    if (_currentData is null)
                        _currentData = null;
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
                // Preserve last-known-good on reload failure
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
        LoadData();
        OnDataChanged?.Invoke();
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _debounceTimer.Dispose();
    }
}