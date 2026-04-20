using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// TODO(T4): implement JSON read, validation, IMemoryCache, FileSystemWatcher-based hot-reload,
// retry-on-IOException, and populate DashboardLoadError on NotFound/ParseError/ValidationError.
public sealed class DashboardDataService : IDashboardDataService
{
    public event EventHandler? DataChanged;

    public DashboardLoadResult GetCurrent()
    {
        // Stub: return empty result so the app boots even when wwwroot/data.json is absent.
        _ = DataChanged; // suppress unused-event warning until T4 wires it.
        return new DashboardLoadResult(null, null, DateTimeOffset.UtcNow);
    }
}