using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// TODO(T4): implement hot-reload, validation, FileSystemWatcher.
public sealed class DashboardDataService : IDashboardDataService
{
    public event EventHandler? DataChanged;

    public DashboardLoadResult GetCurrent()
    {
        return new DashboardLoadResult(null, null, DateTimeOffset.UtcNow);
    }

    private void OnDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
}