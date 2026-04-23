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
    private DashboardData? _data;
    private string? _error;
    private FileSystemWatcher? _watcher;
    private Timer? _pollingTimer;
    private DateTime _lastWriteTime;
    private Timer? _debounceTimer;
    private readonly object _lock = new();
    private bool _disposed;

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger, IWebHostEnvironment env)
    {
        _logger = logger;

        var configPath = configuration["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";
        _filePath = Path.IsPathRooted(configPath)
            ? configPath
            : Path.Combine(env.ContentRootPath, configPath);

        LoadData();
        SetupFileWatcher();
        SetupPollingFallback();
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;

    private void LoadData()
    {
        const int maxRetries = 3;
        const int retryDelayMs = 200;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    var absolutePath = Path.GetFullPath(_filePath);
                    _data = null;
                    _error = $"Dashboard data file not found. Expected location: {absolutePath}";
                    _logger.LogWarning("Dashboard data file not found: {Path}", absolutePath);
                    return;
                }

                var json = File.ReadAllText(_filePath);

                if (json.Length > 1_048_576) // 1 MB warning
                {
                    _logger.LogWarning("Dashboard data file is unusually large ({Size} bytes)", json.Length);
                }

                _data = JsonSerializer.Deserialize<DashboardData>(json);
                if (_data is null)
                {
                    _error = "Error reading dashboard data: File is empty or contains only 'null'.";
                    return;
                }
                _error = null;
                _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);
                return;
            }
            catch (IOException) when (attempt < maxRetries - 1)
            {
                Thread.Sleep(retryDelayMs);
            }
            catch (JsonException ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogError(ex, "Failed to parse dashboard JSON");
                return;
            }
            catch (Exception ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogError(ex, "Unexpected error loading dashboard data");
                return;
            }
        }

        _data = null;
        _error = $"Error reading dashboard data: File is locked and could not be read after {maxRetries} attempts.";
    }

    private void SetupFileWatcher()
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            var fileName = Path.GetFileName(_filePath);

            if (directory == null) return;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
            _watcher.Renamed += (_, _) => DebouncedReload();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FileSystemWatcher setup failed; relying on polling fallback");
        }
    }

    private void SetupPollingFallback()
    {
        _pollingTimer = new Timer(_ => PollForChanges(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    private void PollForChanges()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                // File was deleted — transition to error state
                if (_data is not null || _error is null || !_error.Contains("not found"))
                {
                    DebouncedReload();
                }
                return;
            }

            var currentWriteTime = File.GetLastWriteTimeUtc(_filePath);
            if (currentWriteTime != _lastWriteTime)
            {
                DebouncedReload();
            }
        }
        catch
        {
            // Polling is best-effort
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        DebouncedReload();
    }

    private void DebouncedReload()
    {
        lock (_lock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ =>
            {
                LoadData();
                OnDataChanged?.Invoke();
            }, null, 300, Timeout.Infinite);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _watcher?.Dispose();
        _pollingTimer?.Dispose();
        _debounceTimer?.Dispose();
    }
}
