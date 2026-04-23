using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDisposable
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly object _lock = new();
    private FileSystemWatcher? _watcher;
    private Timer? _pollingTimer;
    private Timer? _debounceTimer;
    private DateTime _lastWriteTime;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardData? Data { get; private set; }
    public bool IsError { get; private set; }
    public string ErrorMessage { get; private set; } = "";

    public event Action? OnDataChanged;

    public DashboardDataService(
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<DashboardDataService> logger)
    {
        _env = env;
        _config = config;
        _logger = logger;
    }

    private string GetDataFilePath()
    {
        var configPath = _config["Dashboard:DataFilePath"] ?? "wwwroot/data/data.json";
        return Path.GetFullPath(configPath, _env.ContentRootPath);
    }

    public async Task LoadAsync()
    {
        var filePath = GetDataFilePath();

        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Data file not found at {Path}", filePath);
                IsError = true;
                ErrorMessage = $"Dashboard data file not found. Expected location: {filePath}";
                return;
            }

            var json = await ReadFileWithRetryAsync(filePath);
            Data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (Data is null)
            {
                _logger.LogError("data.json deserialized to null");
                IsError = true;
                ErrorMessage = "Error reading dashboard data: file deserialized to null.";
                return;
            }

            IsError = false;
            ErrorMessage = "";
            _lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            _logger.LogInformation("Dashboard data loaded: {Title}", Data.Title);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data file");
            IsError = true;
            ErrorMessage = $"Error reading dashboard data: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading data file");
            IsError = true;
            ErrorMessage = $"Error reading dashboard data: {ex.Message}";
        }

        SetupFileWatcher();
        SetupPolling();
    }

    private static async Task<string> ReadFileWithRetryAsync(string path, int maxRetries = 3, int delayMs = 200)
    {
        for (var i = 0; i < maxRetries - 1; i++)
        {
            try
            {
                return await File.ReadAllTextAsync(path);
            }
            catch (IOException)
            {
                await Task.Delay(delayMs);
            }
        }
        return await File.ReadAllTextAsync(path);
    }

    private void SetupFileWatcher()
    {
        try
        {
            var filePath = GetDataFilePath();
            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return;

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _watcher.Changed += (s, e) => DebounceReload();
            _watcher.Created += (s, e) => DebounceReload();
            _watcher.Renamed += (s, e) => DebounceReload();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FileSystemWatcher setup failed; relying on polling");
        }
    }

    private void DebounceReload()
    {
        var debounceMs = _config.GetValue("Dashboard:AutoRefreshDebounceMs", 300);
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(async _ =>
        {
            await LoadAsync();
            OnDataChanged?.Invoke();
        }, null, debounceMs, Timeout.Infinite);
    }

    private void SetupPolling()
    {
        _pollingTimer = new Timer(_ =>
        {
            try
            {
                var filePath = GetDataFilePath();
                if (File.Exists(filePath))
                {
                    var currentWriteTime = File.GetLastWriteTimeUtc(filePath);
                    if (currentWriteTime != _lastWriteTime)
                        DebounceReload();
                }
                else if (Data is not null)
                {
                    DebounceReload();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Polling check error");
            }
        }, null, 5000, 5000);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollingTimer?.Dispose();
        _debounceTimer?.Dispose();
    }
}