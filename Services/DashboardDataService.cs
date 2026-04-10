using System.Security.Cryptography;
using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDashboardDataService
{
    private readonly string _filePath;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // File-watching infrastructure
    private FileSystemWatcher? _watcher;
    private Timer? _pollingTimer;
    private CancellationTokenSource? _debounceCts;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private string? _lastFileHash;

    public DashboardData? Data { get; private set; }
    public string? LoadError { get; private set; }
    public bool IsLoaded => Data is not null;
    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _filePath = configuration.GetValue<string>("DashboardDataPath")
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "data.json");
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    public async Task LoadAsync()
    {
        _logger.LogInformation("Loading dashboard data from {Path}", _filePath);

        if (!File.Exists(_filePath))
        {
            LoadError = $"Dashboard data file not found. Expected at: {_filePath}";
            _logger.LogError(LoadError);
            OnDataChanged?.Invoke();
            return;
        }

        var fileBytes = await ReadFileBytesWithRetryAsync();
        if (fileBytes is null)
        {
            LoadError = $"Failed to read data file after retries: {_filePath}";
            _logger.LogError(LoadError);
            OnDataChanged?.Invoke();
            return;
        }

        _lastFileHash = ComputeHash(fileBytes);
        _logger.LogDebug("Initial file hash: {Hash}", _lastFileHash);

        try
        {
            var data = JsonSerializer.Deserialize<DashboardData>(
                (ReadOnlySpan<byte>)fileBytes, _jsonOptions);

            if (data?.Project is null)
            {
                LoadError = "Invalid data: missing required 'project' section";
                _logger.LogError(LoadError);
            }
            else
            {
                Data = data;
                LoadError = null;
                ValidateData();
                _logger.LogInformation("Dashboard data loaded successfully.");
            }
        }
        catch (JsonException ex)
        {
            LoadError = $"Error loading data: {ex.Message}";
            _logger.LogError(ex, "Failed to deserialize {Path}", _filePath);
        }

        OnDataChanged?.Invoke();

        StartWatcher();
        StartPolling();
    }

    private async Task ReloadAsync()
    {
        if (!_loadLock.Wait(0))
        {
            _logger.LogDebug("Reload already in progress, skipping.");
            return;
        }

        try
        {
            if (!File.Exists(_filePath))
            {
                _logger.LogWarning("Data file not found during reload: {Path}", _filePath);
                return;
            }

            var fileBytes = await ReadFileBytesWithRetryAsync();
            if (fileBytes is null)
            {
                _logger.LogWarning("Failed to read file during reload after retries.");
                return;
            }

            var newHash = ComputeHash(fileBytes);
            if (newHash == _lastFileHash)
            {
                _logger.LogDebug("File hash unchanged, skipping reload.");
                return;
            }

            _lastFileHash = newHash;

            try
            {
                var data = JsonSerializer.Deserialize<DashboardData>(
                    (ReadOnlySpan<byte>)fileBytes, _jsonOptions);

                if (data?.Project is null)
                {
                    LoadError = "Invalid data: missing required 'project' section";
                    _logger.LogError(LoadError);
                }
                else
                {
                    Data = data;
                    LoadError = null;
                    ValidateData();
                    _logger.LogInformation("Dashboard data reloaded successfully.");
                }
            }
            catch (JsonException ex)
            {
                LoadError = $"Error loading data: {ex.Message}";
                _logger.LogError(ex, "Failed to deserialize data.json during reload.");
            }

            OnDataChanged?.Invoke();
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private void QueueDebouncedReload()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500, token);
                await ReloadAsync();
            }
            catch (TaskCanceledException)
            {
                // Debounce was reset by a newer event - expected behavior
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during debounced reload.");
            }
        }, token);
    }

    private void StartWatcher()
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath));
        var fileName = Path.GetFileName(_filePath);

        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            _logger.LogWarning(
                "Cannot start FileSystemWatcher: directory '{Directory}' does not exist. Relying on polling fallback.",
                directory);
            return;
        }

        _watcher = new FileSystemWatcher(directory)
        {
            Filter = fileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = false
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Error += OnWatcherError;

        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("FileSystemWatcher started for {Path}", _filePath);
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File change detected: {ChangeType} {Path}", e.ChangeType, e.FullPath);
        QueueDebouncedReload();
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "FileSystemWatcher error. Polling fallback will continue monitoring.");
    }

    private void StartPolling()
    {
        _pollingTimer = new Timer(
            callback: _ => PollForChanges(),
            state: null,
            dueTime: TimeSpan.FromSeconds(5),
            period: TimeSpan.FromSeconds(5));
        _logger.LogInformation("Polling fallback started with 5-second interval.");
    }

    private void PollForChanges()
    {
        try
        {
            if (!File.Exists(_filePath))
                return;

            var fileBytes = File.ReadAllBytes(_filePath);
            var currentHash = ComputeHash(fileBytes);

            if (currentHash != _lastFileHash)
            {
                _logger.LogInformation("Polling detected file change (hash mismatch). Triggering reload.");
                QueueDebouncedReload();
            }
        }
        catch (IOException ex)
        {
            _logger.LogDebug(ex, "Polling skipped: file is locked.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during polling check.");
        }
    }

    private async Task<byte[]?> ReadFileBytesWithRetryAsync(int maxRetries = 3, int delayMs = 200)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var stream = new FileStream(
                    _filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    bufferSize: 4096,
                    useAsync: true);

                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch (IOException ex) when (attempt < maxRetries)
            {
                _logger.LogDebug(ex,
                    "File read attempt {Attempt}/{Max} failed, retrying in {Delay}ms",
                    attempt, maxRetries, delayMs);
                await Task.Delay(delayMs);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex,
                    "File read failed after {Max} attempts: {Path}",
                    maxRetries, _filePath);
            }
        }

        return null;
    }

    private void ValidateData()
    {
        if (Data is null) return;

        if (string.IsNullOrWhiteSpace(Data.Project?.Name))
        {
            _logger.LogWarning("Validation: Project name is empty or missing.");
        }

        var validStatuses = new[] { "On Track", "At Risk", "Off Track" };
        if (Data.Project is not null && !validStatuses.Contains(Data.Project.Status))
        {
            _logger.LogWarning(
                "Validation: Unknown project status '{Status}'. Expected one of: {Valid}",
                Data.Project.Status, string.Join(", ", validStatuses));
        }

        foreach (var milestone in Data.Milestones ?? [])
        {
            if (!DateOnly.TryParse(milestone.Date, out _))
            {
                _logger.LogWarning(
                    "Validation: Milestone '{Title}' has unparseable date '{Date}'",
                    milestone.Title, milestone.Date);
            }

            var validMilestoneStatuses = new[] { "Completed", "In Progress", "Upcoming", "Delayed" };
            if (!validMilestoneStatuses.Contains(milestone.Status))
            {
                _logger.LogWarning(
                    "Validation: Milestone '{Title}' has unknown status '{Status}'",
                    milestone.Title, milestone.Status);
            }
        }

        foreach (var item in Data.InProgress ?? [])
        {
            if (item.PercentComplete < 0 || item.PercentComplete > 100)
            {
                _logger.LogWarning(
                    "Validation: In-progress item '{Title}' has out-of-range percentComplete: {Pct}",
                    item.Title, item.PercentComplete);
            }
        }

        var validTrends = new[] { "up", "down", "stable" };
        foreach (var metric in Data.Metrics ?? [])
        {
            if (!validTrends.Contains(metric.Trend))
            {
                _logger.LogWarning(
                    "Validation: Metric '{Label}' has unknown trend '{Trend}'",
                    metric.Label, metric.Trend);
            }
        }
    }

    private static string ComputeHash(byte[] content)
    {
        var hashBytes = SHA256.HashData(content);
        return Convert.ToHexString(hashBytes);
    }

    public void Dispose()
    {
        _loadLock.Dispose();
        _logger.LogInformation("DashboardDataService disposed.");
    }
}