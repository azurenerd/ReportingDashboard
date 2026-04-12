using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading and managing dashboard configuration from data.json
/// </summary>
public interface IDashboardDataService
{
    /// <summary>
    /// Load and parse dashboard configuration from data.json file
    /// </summary>
    /// <returns>Deserialized DashboardConfig object</returns>
    /// <exception cref="FileNotFoundException">If data.json does not exist</exception>
    /// <exception cref="JsonException">If JSON is invalid</exception>
    /// <exception cref="InvalidOperationException">If schema validation fails</exception>
    Task<DashboardConfig> GetDashboardConfigAsync();

    /// <summary>
    /// Refresh the cached configuration by re-reading data.json from disk
    /// </summary>
    Task RefreshAsync();

    /// <summary>
    /// Get the last modification time of data.json file
    /// </summary>
    /// <returns>DateTime in UTC of last file modification</returns>
    DateTime GetLastModifiedTime();
}