#nullable enable

using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service interface for loading and managing dashboard data from data.json file.
/// Handles JSON deserialization, schema validation, and data caching.
/// </summary>
public interface IDashboardDataService
{
    /// <summary>
    /// Get the dashboard configuration, loading from disk if not cached.
    /// </summary>
    Task<DashboardConfig> GetDashboardConfigAsync();

    /// <summary>
    /// Force a refresh of the dashboard configuration from disk, clearing cache.
    /// </summary>
    Task RefreshAsync();

    /// <summary>
    /// Get the last modified time of the data.json file.
    /// </summary>
    DateTime GetLastModifiedTime();
}