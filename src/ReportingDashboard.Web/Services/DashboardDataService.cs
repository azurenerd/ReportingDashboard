using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// STUB — T3 will implement real file I/O, parsing, validation, caching, and FileSystemWatcher hot-reload.
public sealed class DashboardDataService : IDashboardDataService
{
    public event EventHandler? DataChanged;

    public DashboardLoadResult GetCurrent()
    {
        // Intentionally returns NotFound in the T1 stub so downstream error-path rendering
        // can be exercised before T3 lands. T3 replaces this body with cached load result.
        _ = DataChanged;
        return new DashboardLoadResult(
            Data: null,
            Error: new DashboardLoadError(
                FilePath: "wwwroot/data.json",
                Message: "Stub DashboardDataService — T3 will implement real file load.",
                Line: null,
                Column: null,
                Kind: "NotFound"),
            LoadedAt: DateTimeOffset.UtcNow);
    }
}