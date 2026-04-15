using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DataService : IDisposable
{
    private const int ExpectedSchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataFilePath;
    private readonly object _lock = new();
    private DashboardData? _currentData;
    private string? _currentError;
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private bool _disposed;

    public event Action? OnDataChanged;

    public DataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.WebRootPath, "data.json");
        _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);

        LoadData();
        InitializeFileWatcher(env.WebRootPath);
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
        var data = GetData();
        if (data?.NowDateOverride is string dateStr
            && !string.IsNullOrWhiteSpace(dateStr)
            && DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out var parsed))
        {
            return parsed;
        }
        return DateOnly.FromDateTime(DateTime.Today);
    }

    public string GetCurrentMonthName()
    {
        var data = GetData();
        if (!string.IsNullOrWhiteSpace(data?.CurrentMonthOverride))
        {
            return data!.CurrentMonthOverride!;
        }
        return GetEffectiveDate().ToString("MMM");
    }

    /// <summary>
    /// Reads the JSON file from disk. Throws IOException if the file is locked.
    /// Returns null if the file does not exist.
    /// </summary>
    private string? ReadJsonFile()
    {
        if (!File.Exists(_dataFilePath))
            return null;
        return File.ReadAllText(_dataFilePath);
    }

    /// <summary>
    /// Parses JSON string and updates internal state. Called after file I/O is complete.
    /// </summary>
    private void ParseAndApply(string? json)
    {
        if (json is null)
        {
            lock (_lock)
            {
                _currentData = null;
                _currentError = "No data.json found. Place a valid data.json file in the wwwroot/ directory.";
            }
            return;
        }

        try
        {
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                lock (_lock)
                {
                    _currentError = "data.json contains invalid JSON: deserialized to null.";
                }
                return;
            }

            if (data.SchemaVersion != ExpectedSchemaVersion)
            {
                lock (_lock)
                {
                    _currentError = $"data.json schemaVersion is {data.SchemaVersion}, expected 1. Update your data.json to match the current schema.";
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
            }
        }
    }

    private void LoadData()
    {
        try
        {
            var json = ReadJsonFile();
            ParseAndApply(json);
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _currentError = $"Error reading data.json: {ex.Message}";
            }
        }
    }

    private void InitializeFileWatcher(string webRootPath)
    {
        try
        {
            _watcher = new FileSystemWatcher(webRootPath)
            {
                Filter = "data.json",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;

            Console.WriteLine("[DataService] FileSystemWatcher started on " + webRootPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DataService] WARNING: FileSystemWatcher failed to start: {ex.Message}. Live reload disabled; use F5 to refresh.");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"[DataService] data.json {e.ChangeType}, scheduling reload...");
        _debounceTimer?.Change(500, Timeout.Infinite);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        Console.WriteLine($"[DataService] data.json renamed, scheduling reload...");
        _debounceTimer?.Change(500, Timeout.Infinite);
    }

    private void OnDebounceElapsed(object? state)
    {
        Console.WriteLine("[DataService] Debounce elapsed, reloading data.json...");

        // Retry with backoff for file-lock contention (editors doing atomic saves)
        const int maxRetries = 3;
        string? json = null;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                json = ReadJsonFile();
                break;
            }
            catch (IOException) when (attempt < maxRetries - 1)
            {
                Thread.Sleep(100 * (attempt + 1));
            }
            catch (IOException ex)
            {
                lock (_lock)
                {
                    _currentError = $"Error reading data.json: {ex.Message}";
                }
                NotifySubscribers();
                return;
            }
        }

        ParseAndApply(json);

        var error = GetError();
        if (error is not null)
            Console.WriteLine($"[DataService] Reload completed with error: {error}");
        else
            Console.WriteLine("[DataService] Reload successful.");

        NotifySubscribers();
    }

    private void NotifySubscribers()
    {
        var handler = OnDataChanged;
        if (handler is not null)
        {
            foreach (var subscriber in handler.GetInvocationList())
            {
                try
                {
                    ((Action)subscriber)();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DataService] OnDataChanged subscriber threw: {ex.Message}");
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnFileChanged;
            _watcher.Created -= OnFileChanged;
            _watcher.Renamed -= OnFileRenamed;
            _watcher.Dispose();
            _watcher = null;
        }

        if (_debounceTimer is not null)
        {
            _debounceTimer.Dispose();
            _debounceTimer = null;
        }
    }
}