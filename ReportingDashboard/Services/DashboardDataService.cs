using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public interface IDashboardDataService : IDisposable
{
    DashboardData? GetData();
    string? GetError();
    event Action? OnDataChanged;
}

public sealed class DashboardDataService : IDashboardDataService
{
    private readonly string _filePath;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly object _lock = new();

    private DashboardData? _data;
    private string? _error;
    private DateTime _lastWriteTimeUtc;

    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private Timer? _pollingTimer;
    private bool _disposed;

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _logger = logger;

        var configuredPath = configuration["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";

        // Try content root first (development), then base directory (published)
        var contentRootPath = Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
        var baseDirectoryPath = Path.Combine(AppContext.BaseDirectory, configuredPath);

        if (Path.IsPathRooted(configuredPath))
        {
            _filePath = configuredPath;
        }
        else if (File.Exists(contentRootPath))
        {
            _filePath = contentRootPath;
        }
        else
        {
            _filePath = baseDirectoryPath;
        }

        LoadData();
        SetupFileWatcher();
        SetupPollingFallback();
    }

    public DashboardData? GetData()
    {
        lock (_lock)
        {
            return _data;
        }
    }

    public string? GetError()
    {
        lock (_lock)
        {
            return _error;
        }
    }

    private void LoadData()
    {
        lock (_lock)
        {
            var absolutePath = Path.GetFullPath(_filePath);

            if (!File.Exists(_filePath))
            {
                _data = null;
                _error = $"Dashboard data file not found. Expected location: {absolutePath}";
                _logger.LogWarning("Data file not found: {Path}", absolutePath);
                return;
            }

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var json = File.ReadAllText(_filePath);

                    if (json.Length > 1_048_576)
                    {
                        _logger.LogWarning("Data file exceeds 1 MB ({Size} bytes)", json.Length);
                    }

                    _data = JsonSerializer.Deserialize<DashboardData>(json);
                    _error = null;
                    _lastWriteTimeUtc = File.GetLastWriteTimeUtc(_filePath);
                    return;
                }
                catch (IOException) when (attempt < 3)
                {
                    Thread.Sleep(200);
                }
                catch (JsonException ex)
                {
                    _data = null;
                    _error = $"Error reading dashboard data: {ex.Message}";
                    _logger.LogError(ex, "JSON parse error");
                    return;
                }
                catch (Exception ex)
                {
                    _data = null;
                    _error = $"Error reading dashboard data: {ex.Message}";
                    _logger.LogError(ex, "Unexpected error loading data");
                    return;
                }
            }
        }
    }

    private void SetupFileWatcher()
    {
        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath));
            var fileName = Path.GetFileName(_filePath);

            if (directory == null || !Directory.Exists(directory))
                return;

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileEvent;
            _watcher.Created += OnFileEvent;
            _watcher.Renamed += (_, _) => ScheduleReload();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to setup FileSystemWatcher");
        }
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        ScheduleReload();
    }

    private void ScheduleReload()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            LoadData();
            OnDataChanged?.Invoke();
        }, null, 300, Timeout.Infinite);
    }

    private void SetupPollingFallback()
    {
        _pollingTimer = new Timer(_ =>
        {
            try
            {
                if (!File.Exists(_filePath)) return;

                var currentWriteTime = File.GetLastWriteTimeUtc(_filePath);
                if (currentWriteTime != _lastWriteTimeUtc)
                {
                    LoadData();
                    OnDataChanged?.Invoke();
                }
            }
            catch
            {
                // Swallow polling errors silently
            }
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _watcher?.Dispose();
        _debounceTimer?.Dispose();
        _pollingTimer?.Dispose();
    }
}