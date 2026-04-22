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
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 200;
    private const int DebounceMs = 300;
    private const int PollingIntervalMs = 5000;
    private const long MaxFileSizeWarningBytes = 1_048_576; // 1 MB

    private readonly string _filePath;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly object _lock = new();
    private DashboardData? _data;
    private string? _error;
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private Timer? _pollingTimer;
    private DateTime _lastWriteTime;
    private bool _disposed;

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration config, IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        var relativePath = config["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";
        _filePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, relativePath));

        _logger.LogInformation("Dashboard data file path: {FilePath}", _filePath);

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
                    _logger.LogWarning("Dashboard data file not found at {FilePath}", _filePath);
                    return;
                }

                var fileInfo = new FileInfo(_filePath);
                if (fileInfo.Length > MaxFileSizeWarningBytes)
                {
                    _logger.LogWarning("Dashboard data file is larger than 1 MB ({Size} bytes). Loading may be slow.", fileInfo.Length);
                }

                _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);

                string json = ReadFileWithRetry();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                _data = JsonSerializer.Deserialize<DashboardData>(json, options);
                _error = null;
                _logger.LogInformation("Dashboard data loaded successfully.");
            }
            catch (FileNotFoundException)
            {
                _data = null;
                _error = $"Dashboard data file not found. Expected location: {_filePath}";
                _logger.LogWarning("Dashboard data file not found at {FilePath}", _filePath);
            }
            catch (JsonException ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogError(ex, "JSON parse error reading dashboard data from {FilePath}", _filePath);
            }
            catch (IOException ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogError(ex, "IO error reading dashboard data from {FilePath}", _filePath);
            }
            catch (Exception ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                _logger.LogError(ex, "Unexpected error reading dashboard data from {FilePath}", _filePath);
            }
        }
    }

    private string ReadFileWithRetry()
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (IOException) when (attempt < MaxRetries)
            {
                _logger.LogWarning("File read attempt {Attempt} of {MaxRetries} failed, retrying in {Delay}ms...",
                    attempt, MaxRetries, RetryDelayMs);
                Thread.Sleep(RetryDelayMs);
            }
        }

        // Final attempt — let the exception propagate
        using var finalStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var finalReader = new StreamReader(finalStream);
        return finalReader.ReadToEnd();
    }

    private void SetupFileWatcher()
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            var fileName = Path.GetFileName(_filePath);

            if (string.IsNullOrEmpty(directory))
            {
                _logger.LogWarning("Cannot determine directory for file watcher. Polling only.");
                return;
            }

            if (!Directory.Exists(directory))
            {
                _logger.LogWarning("Directory {Directory} does not exist. File watcher not started. Polling only.", directory);
                return;
            }

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;
            _watcher.Error += OnWatcherError;

            _logger.LogInformation("File watcher started for {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set up file watcher. Falling back to polling only.");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        DebounceReload();
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        DebounceReload();
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "FileSystemWatcher error occurred.");
    }

    private void DebounceReload()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            _logger.LogInformation("Debounced file change detected. Reloading dashboard data.");
            LoadData();
            OnDataChanged?.Invoke();
        }, null, DebounceMs, Timeout.Infinite);
    }

    private void SetupPolling()
    {
        _pollingTimer = new Timer(_ =>
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    if (_data != null || _error == null)
                    {
                        _logger.LogWarning("Dashboard data file no longer exists at {FilePath}", _filePath);
                        LoadData();
                        OnDataChanged?.Invoke();
                    }
                    return;
                }

                var currentWriteTime = File.GetLastWriteTimeUtc(_filePath);
                if (currentWriteTime != _lastWriteTime)
                {
                    _logger.LogInformation("Polling detected file change. Reloading dashboard data.");
                    LoadData();
                    OnDataChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during polling check.");
            }
        }, null, PollingIntervalMs, PollingIntervalMs);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnFileChanged;
            _watcher.Created -= OnFileChanged;
            _watcher.Renamed -= OnFileRenamed;
            _watcher.Error -= OnWatcherError;
            _watcher.Dispose();
            _watcher = null;
        }

        _debounceTimer?.Dispose();
        _debounceTimer = null;

        _pollingTimer?.Dispose();
        _pollingTimer = null;

        GC.SuppressFinalize(this);
    }
}
