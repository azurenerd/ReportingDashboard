namespace AgentSquad.Runner.Services;

using AgentSquad.Runner.Models;

/// <summary>
/// Service contract for loading and caching dashboard data from JSON.
/// Registered as Singleton in DI container. Must be initialized via InitializeAsync()
/// before application startup completes.
/// </summary>
public interface IProjectDataService
{
    /// <summary>
    /// Initialize dashboard data from data.json file at application startup.
    /// Must be called in Program.cs before app.Run().
    /// 
    /// Reads from: ContentRootPath/data/data.json
    /// Deserializes JSON to ProjectDashboard using System.Text.Json (case-insensitive).
    /// Validates schema and calculates metrics on successful load.
    /// Logs success/failure to console.
    /// </summary>
    /// <exception cref="FileNotFoundException">If data.json not found at ContentRootPath/data/data.json</exception>
    /// <exception cref="JsonException">If JSON malformed or schema validation fails</exception>
    /// <exception cref="InvalidOperationException">If deserialized data is null</exception>
    Task InitializeAsync();

    /// <summary>
    /// Get the cached ProjectDashboard (immutable reference).
    /// Safe to call multiple times; returns same reference until RefreshAsync() called.
    /// 
    /// Typical usage in Blazor components:
    /// protected override async Task OnInitializedAsync()
    /// {
    ///     try { Dashboard = DataService.GetDashboard(); }
    ///     catch (InvalidOperationException) { /* not yet initialized */ }
    /// }
    /// </summary>
    /// <returns>Immutable ProjectDashboard cached from last successful InitializeAsync()</returns>
    /// <exception cref="InvalidOperationException">If InitializeAsync() has not been called or failed</exception>
    ProjectDashboard GetDashboard();

    /// <summary>
    /// Reload data from disk and update cache (Phase 2, placeholder for now).
    /// When implemented, will support hot-reload via file watcher + SignalR.
    /// </summary>
    Task RefreshAsync();

    /// <summary>
    /// True if data loaded successfully from JSON and cache populated.
    /// Check this before calling GetDashboard() to handle uninitialized state.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// User-friendly error message from last InitializeAsync() attempt.
    /// Null if load succeeded. Displayed to user via error alert in DashboardPage.razor.
    /// Examples:
    ///   "Data file not found: data/data.json. Create file with sample data."
    ///   "JSON syntax error: Unexpected token at line 42, column 15."
    ///   "Validation failed: ProjectName is required."
    /// </summary>
    string? LastError { get; }
}