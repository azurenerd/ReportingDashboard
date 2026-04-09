using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Services;

public class ProjectDashboardState : IAsyncDisposable
{
    private readonly DataConfigurationService _configService;
    private readonly ILogger<ProjectDashboardState> _logger;
    private System.Timers.Timer? _refreshTimer;
    private const string DataFilePath = "wwwroot/data.json";

    public ProjectData? CurrentProject { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? LoadError { get; private set; }

    public event Action? OnStateChanged;

    public ProjectDashboardState(
        DataConfigurationService configService,
        ILogger<ProjectDashboardState> logger)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (!_configService.FileExists(DataFilePath))
            {
                LoadError = $"Critical Error: data.json not found at {DataFilePath}\n\n" +
                    "Recovery steps:\n" +
                    "1. Ensure data.json exists in wwwroot/ folder\n" +
                    "2. Verify file has valid JSON structure\n" +
                    "3. Restart application";
                IsLoaded = false;
                return;
            }

            CurrentProject = await _configService.LoadConfigurationAsync(DataFilePath);
            IsLoaded = true;
            LoadError = null;
            _logger.LogInformation("Dashboard initialized successfully");
            OnStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            LoadError = $"Error: {ex.Message}";
            IsLoaded = false;
            _logger.LogError(ex, "Failed to initialize dashboard");
            OnStateChanged?.Invoke();
        }
    }

    public async Task RefreshDataAsync()
    {
        try
        {
            CurrentProject = await _configService.LoadConfigurationAsync(DataFilePath);
            LoadError = null;
            _logger.LogInformation("Dashboard data refreshed");
            OnStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            LoadError = $"Refresh error: {ex.Message}";
            _logger.LogError(ex, "Failed to refresh dashboard data");
            OnStateChanged?.Invoke();
        }
    }

    public void SetRefreshInterval(TimeSpan interval)
    {
        if (_refreshTimer != null)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            _refreshTimer = null;
        }

        if (interval.TotalMilliseconds > 0)
        {
            _refreshTimer = new System.Timers.Timer(interval.TotalMilliseconds);
            _refreshTimer.Elapsed += async (s, e) => await RefreshDataAsync();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();
            _logger.LogInformation("Refresh timer started: {IntervalSeconds}s", interval.TotalSeconds);
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        await Task.CompletedTask;
    }
}