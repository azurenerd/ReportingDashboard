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
    private DashboardData? _data;
    private string? _error;
    private FileSystemWatcher? _watcher;
    private Timer? _pollingTimer;
    private DateTime _lastWriteTime;
    private CancellationTokenSource? _debounceCts;
    private readonly object _lock = new();

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration)
    {
        var configPath = configuration["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";
        if (Path.IsPathRooted(configPath))
        {
            _filePath = configPath;
        }
        else
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), configPath);
        }

        LoadData();
        SetupFileWatcher();
        SetupPolling();
    }

    public DashboardData? GetData() => _data;

    public string? GetError() => _error;

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
                    return;
                }

                var json = ReadWithRetry(_filePath);
                _data = JsonSerializer.Deserialize<DashboardData>(json);
                _error = null;
                _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);
            }
            catch (JsonException ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
            }
            catch (IOException ex)
            {
                // Keep previous data on transient IO error
                _error = $"Error reading dashboard data: {ex.Message}";
            }
            catch (Exception ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
            }
        }
    }

    private static string ReadWithRetry(string path, int retries = 3, int delayMs = 200)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException) when (i < retries - 1)
            {
                Thread.Sleep(delayMs);
            }
        }
        return File.ReadAllText(path);
    }

    private void SetupFileWatcher()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            var file = Path.GetFileName(_filePath);
            if (dir == null) return;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _watcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };
            _watcher.Changed += (_, _) => DebouncedReload();
            _watcher.Created += (_, _) => DebouncedReload();
            _watcher.Renamed += (_, _) => DebouncedReload();
        }
        catch
        {
            // Watcher may fail on some systems; polling is the fallback
        }
    }

    private void DebouncedReload()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        Task.Delay(300, token).ContinueWith(_ =>
        {
            if (!token.IsCancellationRequested)
            {
                LoadData();
                OnDataChanged?.Invoke();
            }
        }, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }

    private void SetupPolling()
    {
        _pollingTimer = new Timer(_ =>
        {
            try
            {
                if (!File.Exists(_filePath)) return;
                var writeTime = File.GetLastWriteTimeUtc(_filePath);
                if (writeTime != _lastWriteTime)
                {
                    _lastWriteTime = writeTime;
                    LoadData();
                    OnDataChanged?.Invoke();
                }
            }
            catch
            {
                // Swallow; next tick retries
            }
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollingTimer?.Dispose();
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}