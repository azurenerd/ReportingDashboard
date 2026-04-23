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
    private readonly object _lock = new();

    private DashboardData? _data;
    private string? _error;
    private DateTime _lastWriteTime;
    private CancellationTokenSource? _debounceCts;

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
        var directory = Path.GetDirectoryName(_filePath);
        var fileName = Path.GetFileName(_filePath);

        if (directory != null && Directory.Exists(directory))
        {
            try
            {
                _watcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                _watcher.Renamed += OnFileChanged;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FileSystemWatcher could not be initialized; relying on polling fallback");
            }
        }

        // Polling fallback every 5 seconds
        _pollingTimer = new Timer(PollForChanges, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public DashboardData? GetData()
    {
        lock (_lock) return _data;
    }

    public string? GetError()
    {
        lock (_lock) return _error;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        DebounceReload();
    }

    private void PollForChanges(object? state)
    {
        try
        {
            if (!File.Exists(_filePath)) return;

            var currentWriteTime = File.GetLastWriteTimeUtc(_filePath);
            if (currentWriteTime != _lastWriteTime)
            {
                DebounceReload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Polling check failed");
        }
    }

    private void DebounceReload()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);
                if (!token.IsCancellationRequested)
                {
                    LoadData();
                    OnDataChanged?.Invoke();
                }
            }
            catch (TaskCanceledException) { }
        }, token);
    }

    private void LoadData()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath))
            {
                _data = null;
                _error = $"Dashboard data file not found. Expected location: {Path.GetFullPath(_filePath)}";
                _logger.LogWarning("Dashboard data file not found at {Path}", _filePath);
                return;
            }

            const int maxRetries = 3;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var json = File.ReadAllText(_filePath);

                    if (json.Length > 1_048_576) // 1 MB warning
                    {
                        _logger.LogWarning("Dashboard data file is unusually large ({Size} bytes)", json.Length);
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = false,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    };

                    _data = JsonSerializer.Deserialize<DashboardData>(json, options);
                    _error = null;
                    _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);
                    return;
                }
                catch (IOException) when (attempt < maxRetries - 1)
                {
                    Thread.Sleep(200);
                }
                catch (JsonException ex)
                {
                    _data = null;
                    _error = $"Error reading dashboard data: {ex.Message}";
                    _logger.LogError(ex, "Failed to parse dashboard data JSON");
                    return;
                }
                catch (Exception ex)
                {
                    _data = null;
                    _error = $"Error reading dashboard data: {ex.Message}";
                    _logger.LogError(ex, "Unexpected error reading dashboard data");
                    return;
                }
            }
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollingTimer.Dispose();
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
