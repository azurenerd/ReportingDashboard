using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

/// <summary>
/// Reads, deserializes, and caches data.json. T2 will implement fully
/// with FileSystemWatcher hot-reload and error handling.
/// </summary>
public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    /// <summary>The current deserialized dashboard data (null if not yet loaded or file missing).</summary>
    public DashboardData? Data { get; private set; }

    /// <summary>Whether data has been successfully loaded at least once.</summary>
    public bool IsLoaded { get; private set; }

    /// <summary>Error message if data could not be loaded; null when healthy.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Fires when data is reloaded from disk. T2 will implement.</summary>
    public event Action? OnDataChanged;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
        // T2 will add: initial load from wwwroot/data/data.json + FileSystemWatcher setup
    }
}