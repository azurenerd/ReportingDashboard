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
    private readonly FileSystemWatcher? _watcher;
    private readonly Timer _pollingTimer;

    private DashboardData? _data;
    private string? _error;
    private DateTime _lastWriteTime;
    private Timer? _debounceTimer;
    private readonly object _lock = new();

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger, IWebHostEnvironment env)
    {
        _logger = logger;

        var configPath = configuration["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";
        _filePath = Path.IsPathRooted(configPath)
            ? configPath
            : Path.Combine(env.ContentRootPath, configPath);

        LoadData();

        // Set up FileSystemWatcher
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            var fileName = Path.GetFileName(_filePath);
            if (directory != null && Directory.Exists(directory))
            {
                _watcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                _watcher.Renamed += (_, _) => DebouncedReload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FileSystemWatcher could not be initialized; relying on polling fallback");
        }

        // Polling fallback every 5 seconds
        _pollingTimer = new Timer(PollForChanges, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        DebouncedReload();
    }

    private void DebouncedReload()
    {
        lock (_lock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ => LoadData(), null, 300, Timeout.Infinite);
        }
    }

    private void PollForChanges(object? state)
    {
        try
        {
            if (!File.Exists(_filePath)) return;
            var currentWriteTime = File.GetLastWriteTimeUtc(_filePath);
            if (currentWriteTime != _lastWriteTime)
            {
                _lastWriteTime = currentWriteTime;
                LoadData();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Polling check failed");
        }
    }

    private void LoadData()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                var absolutePath = Path.GetFullPath(_filePath);
                _data = null;
                _error = $"Dashboard data file not found. Expected location: {absolutePath}";
                _logger.LogError("Dashboard data file not found at {Path}", absolutePath);
                OnDataChanged?.Invoke();
                return;
            }

            string json = ReadFileWithRetry(_filePath);

            var fileInfo = new FileInfo(_filePath);
            if (fileInfo.Length > 1_000_000)
            {
                _logger.LogWarning("Dashboard data file is unusually large ({Size} bytes)", fileInfo.Length);
            }

            _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);

            var data = JsonSerializer.Deserialize<DashboardData>(json);
            if (data == null)
            {
                _data = null;
                _error = "Error reading dashboard data: deserialization returned null";
                _logger.LogError("Dashboard data deserialized to null");
            }
            else
            {
                _data = data;
                _error = null;
                _logger.LogInformation("Dashboard data loaded successfully: {Title}", data.Project.Title);
            }
        }
        catch (JsonException ex)
        {
            _data = null;
            _error = $"Error reading dashboard data: {ex.Message}";
            _logger.LogError(ex, "Failed to parse dashboard data JSON");
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "IO error reading dashboard data file (may be locked)");
            // Keep previous data/error state on transient IO errors
            return;
        }
        catch (Exception ex)
        {
            _data = null;
            _error = $"Error reading dashboard data: {ex.Message}";
            _logger.LogError(ex, "Unexpected error loading dashboard data");
        }

        OnDataChanged?.Invoke();
    }

    private static string ReadFileWithRetry(string path, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                Thread.Sleep(200);
            }
        }
        return File.ReadAllText(path);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollingTimer.Dispose();
        _debounceTimer?.Dispose();
    }
}