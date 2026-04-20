namespace ReportingDashboard.Web.Services;

/// <summary>
/// Stub implementation. Downstream task T2 will implement file I/O, parsing,
/// validation, caching, and FileSystemWatcher-based hot-reload.
/// </summary>
public sealed class DashboardDataService : IDashboardDataService
{
    public event EventHandler? DataChanged;

    public DashboardLoadResult GetCurrent() => DashboardLoadResult.Empty;

    // Retained to suppress "event never used" warning; T2 will raise on reload.
    internal void RaiseDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
}