using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// TODO(T4): implement hot-reload via FileSystemWatcher (debounced 250ms),
// System.Text.Json deserialization with retry, DashboardDataValidator integration,
// IMemoryCache storage, structured logging. Must never throw to callers.
public sealed class DashboardDataService : IDashboardDataService
{
    public DashboardDataService()
    {
    }

    public event EventHandler? DataChanged;

    public DashboardLoadResult GetCurrent()
    {
        // Stub: returns empty result so app boots even when data.json is missing or not yet wired.
        _ = DataChanged;
        return new DashboardLoadResult(Data: null, Error: null, LoadedAt: DateTimeOffset.UtcNow);
    }
}