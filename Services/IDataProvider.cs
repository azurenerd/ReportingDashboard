namespace AgentSquad.Runner.Services;

/// <summary>
/// Service interface for loading and managing project data from JSON configuration.
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Asynchronously loads project data from wwwroot/data.json, with caching.
    /// </summary>
    /// <returns>Strongly-typed Project model with nested collections.</returns>
    /// <exception cref="FileNotFoundException">Thrown when data.json is not found.</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when JSON is malformed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when data validation fails.</exception>
    Task<Project> LoadProjectDataAsync();

    /// <summary>
    /// Invalidates the cached project data, forcing a reload on next call.
    /// </summary>
    void InvalidateCache();
}