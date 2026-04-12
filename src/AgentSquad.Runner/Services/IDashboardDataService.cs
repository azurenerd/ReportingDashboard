using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service interface for loading and managing dashboard configuration from data.json file.
    /// Handles JSON deserialization, schema validation, and file modification tracking.
    /// </summary>
    public interface IDashboardDataService
    {
        /// <summary>
        /// Load and return parsed dashboard configuration from data.json file.
        /// </summary>
        /// <returns>Deserialized DashboardConfig object with quarters and milestones.</returns>
        /// <exception cref="FileNotFoundException">Thrown if data.json file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if JSON is invalid or schema validation fails.</exception>
        Task<DashboardConfig> GetDashboardConfigAsync();

        /// <summary>
        /// Refresh configuration by re-reading data.json from disk, clearing in-memory cache.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task RefreshAsync();

        /// <summary>
        /// Get the file system modification time of data.json for "Last Updated" timestamp display.
        /// </summary>
        /// <returns>DateTime in UTC of last file modification.</returns>
        DateTime GetLastModifiedTime();
    }
}