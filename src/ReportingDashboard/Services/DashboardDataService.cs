using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public interface IDashboardDataService : IDisposable
{
    DashboardData? GetData();
    string? GetError();
    event Action? OnDataChanged;
}

public class DashboardDataService : IDashboardDataService
{
    private readonly string _filePath;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly object _lock = new();

    private DashboardData? _data;
    private string? _error;
    private FileSystemWatcher? _watcher;
    private Timer? _pollTimer;
    private Timer? _debounceTimer;
    private DateTime _lastWriteTime;

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        var configuredPath = configuration["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";

        _filePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuredPath));

        LoadData();
        SetupFileWatcher();
        SetupPollingFallback();
    }

    public DashboardData? GetData() { lock (_lock) { return _data; } }
    public string? GetError() { lock (_lock) { return _error; } }

    private void LoadData()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _data = null;
                    _error = $"Dashboard data file not found. Expected location: {_filePath}";
                    _logger.LogWarning("Dashboard data file not found: {Path}", _filePath);
                    return;
                }

                var fileInfo = new FileInfo(_filePath);
                if (fileInfo.Length > 1_048_576)
                {
                    _logger.LogWarning("Dashboard data file is unusually large ({Size} bytes): {Path}", fileInfo.Length, _filePath);
                }

                var json = ReadFileWithRetry();
                var data = JsonSerializer.Deserialize<DashboardData>(json);

                if (data is null)
                {
                    _data = null;
                    _error = "Error reading dashboard data: deserialization returned null";
                    return;
                }

                _data = data;
                _error = null;
                _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);
            }
            catch (JsonException ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogWarning(ex, "JSON parse error in dashboard data file");
            }
            catch (Exception ex)
            {
                // Keep previous cached data on transient IO errors
                if (_data is null)
                {
                    _error = $"Error reading dashboard data: {ex.Message}";
                }
                _logger.LogWarning(ex, "Error reading dashboard data file");
            }
        }

        OnDataChanged?.Invoke();
    }

    private string ReadFileWithRetry()
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                return File.ReadAllText(_filePath);
            }
            catch (IOException) when (attempt < 2)
            {
                Thread.Sleep(200);
            }
        }
        return File.ReadAllText(_filePath);
    }

    private void SetupFileWatcher()
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            var fileName = Path.GetFileName(_filePath);

            if (string.IsNullOrEmpty(directory)) return;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName
            };
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += (_, _) => DebouncedReload();
            _watcher.EnableRaisingEvents = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set up FileSystemWatcher; relying on polling fallback");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) => DebouncedReload();

    private void DebouncedReload()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ => LoadData(), null, 300, Timeout.Infinite);
    }

    private void SetupPollingFallback()
    {
        _pollTimer = new Timer(_ =>
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var currentWriteTime = File.GetLastWriteTimeUtc(_filePath);
                    if (currentWriteTime != _lastWriteTime)
                    {
                        DebouncedReload();
                    }
                }
                else if (_data is not null)
                {
                    // File was deleted after being loaded
                    LoadData();
                }
            }
            catch
            {
                // Ignore polling errors silently
            }
        }, null, 5000, 5000);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollTimer?.Dispose();
        _debounceTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}