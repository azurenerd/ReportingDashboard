using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service for loading, parsing, and managing project data from configuration.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Loads project data from data.json file, using cache when available.
        /// </summary>
        /// <returns>The loaded and validated Project data.</returns>
        /// <exception cref="FileNotFoundException">Thrown when data.json file is not found.</exception>
        /// <exception cref="System.Text.Json.JsonException">Thrown when JSON is invalid or malformed.</exception>
        /// <exception cref="InvalidOperationException">Thrown when data validation fails.</exception>
        Task<Project> LoadProjectDataAsync();

        /// <summary>
        /// Invalidates the cached project data, forcing a reload from file on next call.
        /// </summary>
        void InvalidateCache();
    }
}