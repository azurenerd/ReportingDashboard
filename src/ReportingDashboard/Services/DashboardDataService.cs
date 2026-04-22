using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDisposable
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;
    private FileSystemWatcher? _watcher;
    private Timer? _pollTimer;
    private DateTime _lastFileWrite = DateTime.MinValue;
    private CancellationTokenSource? _debounceCts;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public DashboardData? Data { get; private set; }
    public bool IsLoaded => Data is not null && !HasError;
    public bool HasError { get; private set; }
    public string ErrorMessage { get; private set; } = "";

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration config, IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    private string GetDataFilePath()
    {
        var configured = _config["Dashboard:DataFilePath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (Path.IsPathRooted(configured))
                return configured;
            return Path.Combine(_env.ContentRootPath, configured);
        }
        return Path.Combine(_env.WebRootPath, "data", "dashboard-data.json");
    }

    public async Task LoadAsync()
    {
        var filePath = GetDataFilePath();
        var maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Dashboard data file not found at {Path}", filePath);
                    HasError = true;
                    ErrorMessage = $"Dashboard data file not found. Expected location: {filePath}";
                    Data = null;
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

                if (data is null)
                {
                    HasError = true;
                    ErrorMessage = "Dashboard data file is empty or invalid.";
                    Data = null;
                    return;
                }

                Data = data;
                HasError = false;
                ErrorMessage = "";
                _logger.LogInformation("Dashboard data loaded: {Title}", data.Project.Title);
                return;
            }
            catch (IOException) when (attempt < maxRetries)
            {
                _logger.LogWarning("File locked, retry {Attempt}/{Max}", attempt, maxRetries);
                await Task.Delay(300 * attempt);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parse error in dashboard data");
                HasError = true;
                ErrorMessage = $"Error reading dashboard data: {ex.Message}";
                Data = null;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading dashboard data");
                HasError = true;
                ErrorMessage = $"Error reading dashboard data: {ex.Message}";
                Data = null;
                return;
            }
        }
    }

    public void StartWatching()
    {
        var filePath = GetDataFilePath();
        var directory = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileName(filePath);

        if (directory is null) return;

        try
        {
            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _logger.LogInformation("FileSystemWatcher active for {Path}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FileSystemWatcher failed, using polling only");
        }

        _pollTimer = new Timer(PollForChanges, filePath, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        DebouncedReload();
    }

    private void PollForChanges(object? state)
    {
        if (state is not string filePath) return;
        try
        {
            if (!File.Exists(filePath)) return;
            var lastWrite = File.GetLastWriteTimeUtc(filePath);
            if (lastWrite > _lastFileWrite)
            {
                _lastFileWrite = lastWrite;
                DebouncedReload();
            }
        }
        catch { /* ignore polling errors */ }
    }

    private void DebouncedReload()
    {
        lock (_lock)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, token);
                    if (token.IsCancellationRequested) return;
                    await LoadAsync();
                    OnDataChanged?.Invoke();
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during debounced reload");
                }
            }, token);
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollTimer?.Dispose();
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
    }
}