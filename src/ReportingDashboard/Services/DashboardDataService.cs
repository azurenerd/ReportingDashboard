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
    private bool _disposed;

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        var configPath = configuration["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";
        _filePath = Path.IsPathRooted(configPath) ? configPath : Path.Combine(Directory.GetCurrentDirectory(), configPath);

        LoadData();
        SetupFileWatcher();
        SetupPollingFallback();
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;

    private void LoadData()
    {
        var absolutePath = Path.GetFullPath(_filePath);

        if (!File.Exists(absolutePath))
        {
            _data = null;
            _error = $"Dashboard data file not found. Expected location: {absolutePath}";
            _logger.LogWarning("Dashboard data file not found: {Path}", absolutePath);
            return;
        }

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var json = File.ReadAllText(absolutePath);

                if (json.Length > 1_000_000)
                {
                    _logger.LogWarning("Dashboard data file is unusually large ({Size} bytes): {Path}", json.Length, absolutePath);
                }

                _data = JsonSerializer.Deserialize<DashboardData>(json);
                _error = null;
                _lastWriteTime = File.GetLastWriteTimeUtc(absolutePath);
                _logger.LogInformation("Dashboard data loaded successfully from {Path}", absolutePath);
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
                _logger.LogError(ex, "Failed to parse dashboard data from {Path}", absolutePath);
                return;
            }
            catch (Exception ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogError(ex, "Unexpected error loading dashboard data from {Path}", absolutePath);
                return;
            }
        }
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
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += (s, e) => OnFileChanged(s, e);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set up FileSystemWatcher, relying on polling fallback");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
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
                var absolutePath = Path.GetFullPath(_filePath);
                if (!File.Exists(absolutePath)) return;

                var currentWriteTime = File.GetLastWriteTimeUtc(absolutePath);
                if (currentWriteTime > _lastWriteTime)
                {
                    _lastWriteTime = currentWriteTime;
                    LoadData();
                    OnDataChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Polling fallback error");
            }
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
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