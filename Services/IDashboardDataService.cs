using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

/// <summary>
/// Single point of data access for the dashboard. Reads data.json,
/// deserializes it, watches for changes, and notifies subscribers.
/// </summary>
public interface IDashboardDataService : IDisposable
{
    /// <summary>
    /// The current deserialized dashboard data. Null if never successfully loaded.
    /// </summary>
    DashboardData? Data { get; }

    /// <summary>
    /// Human-readable error message from the last failed load attempt.
    /// Null when data is successfully loaded.
    /// </summary>
    string? LoadError { get; }

    /// <summary>
    /// True if Data is non-null (at least one successful load has occurred).
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Fired when Data or LoadError changes. Subscribers must call
    /// InvokeAsync(StateHasChanged) to update the Blazor UI.
    /// </summary>
    event Action? OnDataChanged;

    /// <summary>
    /// Reads and deserializes data.json. Called once at startup.
    /// Starts the FileSystemWatcher and polling timer.
    /// </summary>
    Task LoadAsync();
}