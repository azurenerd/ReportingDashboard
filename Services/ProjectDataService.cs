using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading and managing project data from JSON file.
/// </summary>
public class ProjectDataService
{
    private readonly ILogger<ProjectDataService> logger;
    private readonly IWebHostEnvironment environment;

    public ProjectDataService(ILogger<ProjectDataService> logger, IWebHostEnvironment environment)
    {
        this.logger = logger;
        this.environment = environment;
    }

    /// <summary>
    /// Loads project data from data.json file in wwwroot directory.
    /// </summary>
    /// <returns>ProjectData object if successful, null if file not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when JSON parsing fails.</exception>
    public async Task<ProjectData?> LoadProjectDataAsync()
    {
        try
        {
            var dataPath = Path.Combine(environment.WebRootPath, "data.json");

            if (!File.Exists(dataPath))
            {
                logger.LogWarning("data.json file not found at {DataPath}", dataPath);
                return null;
            }

            var jsonContent = await File.ReadAllTextAsync(dataPath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var projectData = JsonSerializer.Deserialize<ProjectData>(jsonContent, options);

            if (projectData == null)
            {
                logger.LogWarning("Failed to deserialize project data from JSON");
                throw new InvalidOperationException("Project data could not be deserialized from JSON.");
            }

            logger.LogInformation("Successfully loaded project data: {ProjectName}", projectData.ProjectName);
            return projectData;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON parsing error while loading project data");
            throw new InvalidOperationException($"Invalid JSON format in data.json: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "IO error while loading project data");
            throw new InvalidOperationException($"Error reading data.json file: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while loading project data");
            throw new InvalidOperationException($"Unexpected error loading project data: {ex.Message}", ex);
        }
    }
}