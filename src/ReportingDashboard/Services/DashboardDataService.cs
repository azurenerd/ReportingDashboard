using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public interface IDashboardDataService
{
    DashboardData? GetData();
    string? GetError();
    event Action? OnDataChanged;
}

public class DashboardDataService : IDashboardDataService, IDisposable
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private DashboardData? _data;
    private string? _error;
    private FileSystemWatcher? _watcher;
    private Timer? _pollTimer;
    private Timer? _debounceTimer;
    private DateTime _lastWriteTime;

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var relative = configuration["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";
        _filePath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, relative));

        LoadData();
        SetupFileWatcher();
        SetupPolling();
    }

    public DashboardData? GetData()
    {
        lock (_lock) { return _data; }
    }

    public string? GetError()
    {
        lock (_lock) { return _error; }
    }

    private void LoadData()
    {
        if (!File.Exists(_filePath))
        {
            lock (_lock)
            {
                _data = null;
                _error = $"Dashboard data file not found. Expected location: {_filePath}";
            }
            return;
        }

        Exception? lastEx = null;
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<DashboardData>(json);
                lock (_lock)
                {
                    _data = data;
                    _error = null;
                }
                _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);
                return;
            }
            catch (IOException ex)
            {
                lastEx = ex;
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _data = null;
                    _error = $"Error reading dashboard data: {ex.Message}";
                }
                return;
            }
        }

        lock (_lock)
        {
            _data = null;
            _error = $"Error reading dashboard data: {lastEx?.Message}";
        }
    }

    private void SetupFileWatcher()
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            var fileName = Path.GetFileName(_filePath);
            if (directory == null) return;

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
        }
        catch
        {
            // FileSystemWatcher may not be supported; polling will handle it
        }
    }

    private void SetupPolling()
    {
        _pollTimer = new Timer(_ =>
        {
            try
            {
                if (!File.Exists(_filePath)) return;
                var current = File.GetLastWriteTimeUtc(_filePath);
                if (current != _lastWriteTime)
                {
                    ScheduleReload();
                }
            }
            catch
            {
                // Ignore polling errors
            }
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
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

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollTimer?.Dispose();
        _debounceTimer?.Dispose();
    }
}
