using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DataService : IDisposable
{
    private const int ExpectedSchemaVersion = 1;
    private readonly string _dataFilePath;
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private readonly object _lock = new();

    private DashboardData? _currentData;
    private string? _currentError;

    public event Action? OnDataChanged;

    public DataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.WebRootPath, "data.json");
        _debounceTimer = new Timer(_ => ReloadData(), null, Timeout.Infinite, Timeout.Infinite);

        LoadData();

        try
        {
            var dir = Path.GetDirectoryName(_dataFilePath)!;
            if (Directory.Exists(dir))
            {
                _watcher = new FileSystemWatcher(dir, "data.json")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                _watcher.Renamed += (_, _) => OnFileChanged(null, null!);
            }
        }
        catch
        {
            // FileSystemWatcher may fail in some environments
        }
    }

    protected DataService()
    {
        _dataFilePath = string.Empty;
    }

    public virtual DashboardData? GetData()
    {
        lock (_lock) return _currentData;
    }

    public virtual string? GetError()
    {
        lock (_lock) return _currentError;
    }

    public virtual DateOnly GetEffectiveDate()
    {
        var data = GetData();
        if (data?.NowDateOverride is { } ov && !string.IsNullOrWhiteSpace(ov))
        {
            try
            {
                return DateOnly.ParseExact(ov, "yyyy-MM-dd");
            }
            catch
            {
                return DateOnly.FromDateTime(DateTime.Today);
            }
        }
        return DateOnly.FromDateTime(DateTime.Today);
    }

    public virtual string GetCurrentMonthName()
    {
        var data = GetData();
        if (data?.CurrentMonthOverride is { } cmo && !string.IsNullOrWhiteSpace(cmo))
        {
            return cmo;
        }
        return GetEffectiveDate().ToString("MMM");
    }

    private void OnFileChanged(object? sender, FileSystemEventArgs e)
    {
        _debounceTimer?.Change(500, Timeout.Infinite);
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
                    _currentError = "No data.json found. Place a valid data.json file in the wwwroot/ directory and restart.";
                }
                return;
            }

            var json = File.ReadAllText(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json);

            if (data is null)
            {
                lock (_lock)
                {
                    _currentData = null;
                    _currentError = "data.json contains invalid JSON: deserialized to null.";
                }
                return;
            }

            if (data.SchemaVersion != ExpectedSchemaVersion)
            {
                lock (_lock)
                {
                    _currentData = null;
                    _currentError = $"data.json schemaVersion is {data.SchemaVersion}, expected {ExpectedSchemaVersion}. Update your data.json to match the current schema.";
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
                _currentData = null;
                _currentError = $"data.json contains invalid JSON: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _currentData = null;
                _currentError = $"Error reading data.json: {ex.Message}";
            }
        }
    }

    private void ReloadData()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                lock (_lock)
                {
                    _currentError = "data.json file not found.";
                    // Preserve _currentData (last known good) on file-not-found during reload
                }
                FireOnDataChanged();
                return;
            }

            var json = File.ReadAllText(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json);

            if (data is null)
            {
                lock (_lock)
                {
                    _currentError = "data.json contains invalid JSON: deserialized to null.";
                    // Preserve _currentData (last known good)
                }
                FireOnDataChanged();
                return;
            }

            if (data.SchemaVersion != ExpectedSchemaVersion)
            {
                lock (_lock)
                {
                    _currentError = $"data.json schemaVersion is {data.SchemaVersion}, expected {ExpectedSchemaVersion}.";
                    // Preserve _currentData (last known good)
                }
                FireOnDataChanged();
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
                // Preserve _currentData (last known good)
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _currentError = $"Error reloading data.json: {ex.Message}";
                // Preserve _currentData (last known good)
            }
        }

        FireOnDataChanged();
    }

    private void FireOnDataChanged()
    {
        var handlers = OnDataChanged;
        if (handlers != null)
        {
            foreach (var handler in handlers.GetInvocationList())
            {
                try
                {
                    ((Action)handler)();
                }
                catch
                {
                    // Subscriber exceptions must not kill the reload pipeline
                }
            }
        }
    }

    public virtual void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
        _debounceTimer?.Dispose();
        _debounceTimer = null;
    }
}