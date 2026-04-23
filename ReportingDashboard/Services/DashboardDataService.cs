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
    private Timer? _pollTimer;
    private DateTime _lastWriteTime;
    private Timer? _debounceTimer;
    private bool _disposed;

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        var configuredPath = configuration["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";

        _filePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath);

        LoadData();
        SetupFileWatcher();
        SetupPollingFallback();
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;

    private void LoadData()
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _data = null;
                    _error = $"Dashboard data file not found. Expected location: {Path.GetFullPath(_filePath)}";
                    _logger.LogWarning("Dashboard data file not found: {Path}", _filePath);
                    return;
                }

                var json = File.ReadAllText(_filePath);

                if (json.Length > 1_048_576)
                {
                    _logger.LogWarning("Dashboard data file exceeds 1 MB: {Size} bytes", json.Length);
                }

                _data = JsonSerializer.Deserialize<DashboardData>(json);
                _error = null;

                if (File.Exists(_filePath))
                {
                    _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);
                }

                _logger.LogInformation("Dashboard data loaded successfully from {Path}", _filePath);
                return;
            }
            catch (IOException) when (attempt < 2)
            {
                Thread.Sleep(200);
            }
            catch (JsonException ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogError(ex, "JSON deserialization error for {Path}", _filePath);
                return;
            }
            catch (Exception ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogError(ex, "Unexpected error loading dashboard data from {Path}", _filePath);
                return;
            }
        }

        _data = null;
        _error = $"Error reading dashboard data: File is locked and could not be read after 3 attempts.";
    }

    private void SetupFileWatcher()
    {
        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath));
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
            _watcher.Renamed += OnFileRenamed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set up FileSystemWatcher for {Path}", _filePath);
        }
    }

    private void SetupPollingFallback()
    {
        _pollTimer = new Timer(_ =>
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    if (_data != null || (_error != null && !_error.Contains("not found")))
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during polling check");
            }
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        DebouncedReload();
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        DebouncedReload();
    }

    private void DebouncedReload()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            LoadData();
            OnDataChanged?.Invoke();
        }, null, 300, Timeout.Infinite);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _watcher?.Dispose();
        _pollTimer?.Dispose();
        _debounceTimer?.Dispose();
    }
}