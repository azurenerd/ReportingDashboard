using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDisposable
{
    private readonly string _dataFilePath;
    private readonly FileSystemWatcher _fileWatcher;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly object _lock = new();
    private readonly Timer _debounceTimer;
    private DashboardData _currentData;
    private bool _disposed;

    private const int DebounceIntervalMs = 500;
    private const int FileReadRetryCount = 3;
    private const int FileReadRetryDelayMs = 150;

    public event Action? DataChanged;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        _dataFilePath = Path.Combine(env.WebRootPath, "data.json");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        _currentData = LoadData();

        _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);

        var directory = Path.GetDirectoryName(_dataFilePath)!;
        var fileName = Path.GetFileName(_dataFilePath);

        _fileWatcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };

        _fileWatcher.Changed += OnFileChanged;
        _fileWatcher.Created += OnFileChanged;
        _fileWatcher.Renamed += OnFileChanged;

        _logger.LogInformation("DashboardDataService initialized. Watching: {Path}", _dataFilePath);
    }

    public DashboardData GetDashboardData()
    {
        lock (_lock)
        {
            return _currentData;
        }
    }

    private string ReadFileWithRetry()
    {
        for (int attempt = 1; attempt <= FileReadRetryCount; attempt++)
        {
            try
            {
                using var stream = new FileStream(
                    _dataFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (IOException) when (attempt < FileReadRetryCount)
            {
                _logger.LogDebug(
                    "File read attempt {Attempt}/{MaxRetries} failed, retrying in {Delay}ms...",
                    attempt, FileReadRetryCount, FileReadRetryDelayMs);
                Thread.Sleep(FileReadRetryDelayMs);
            }
        }

        using var finalStream = new FileStream(
            _dataFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);
        using var finalReader = new StreamReader(finalStream);
        return finalReader.ReadToEnd();
    }

    private DashboardData LoadData()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                _logger.LogWarning("data.json not found at {Path}. Returning empty data.", _dataFilePath);
                return new DashboardData { ProjectName = "No Data", Summary = "data.json not found." };
            }

            var json = ReadFileWithRetry();
            var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);

            if (data is null)
            {
                _logger.LogWarning("data.json deserialized to null. Returning empty data.");
                return new DashboardData { ProjectName = "Parse Error", Summary = "Failed to parse data.json." };
            }

            _logger.LogInformation("Loaded dashboard data for project: {ProjectName}", data.ProjectName);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data.json from {Path}", _dataFilePath);
            return new DashboardData
            {
                ProjectName = "Load Error",
                Summary = $"Error reading data.json: {ex.Message}"
            };
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer.Change(DebounceIntervalMs, Timeout.Infinite);
    }

    private void OnDebounceElapsed(object? state)
    {
        try
        {
            var newData = LoadData();
            lock (_lock)
            {
                _currentData = newData;
            }

            _logger.LogInformation("data.json reloaded successfully");
            DataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading data.json after file change.");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _fileWatcher.Changed -= OnFileChanged;
        _fileWatcher.Created -= OnFileChanged;
        _fileWatcher.Renamed -= OnFileChanged;
        _fileWatcher.EnableRaisingEvents = false;
        _fileWatcher.Dispose();

        _debounceTimer.Dispose();

        GC.SuppressFinalize(this);
    }
}