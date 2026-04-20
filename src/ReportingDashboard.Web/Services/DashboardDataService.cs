namespace ReportingDashboard.Web.Services;

// Stub - T3 will flesh out with full read/parse/validate/hot-reload behavior.
// For scaffold boot-up, returns a NotFound error so the page still renders.
public sealed class DashboardDataService : IDashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<DashboardDataService> _logger;

    public event EventHandler? DataChanged;

    public DashboardDataService(
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<DashboardDataService> logger)
    {
        _env = env;
        _config = config;
        _logger = logger;
    }

    public DashboardLoadResult GetCurrent()
    {
        var relative = _config["Dashboard:DataFilePath"] ?? "wwwroot/data.json";
        var fullPath = Path.IsPathRooted(relative)
            ? relative
            : Path.Combine(_env.ContentRootPath, relative);

        var error = new DashboardLoadError(
            FilePath: fullPath,
            Message: "Scaffold stub: data loading not yet implemented (T3).",
            Line: null,
            Column: null,
            Kind: "NotFound");

        return new DashboardLoadResult(Data: null, Error: error, LoadedAt: DateTimeOffset.UtcNow);
    }

    // Reserved for future use (suppress unused-event warning).
    private void RaiseDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
}