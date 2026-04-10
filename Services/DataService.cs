using AgentSquad.Runner.Models;
using System.Text.Json;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading and deserializing project data from JSON configuration files.
/// Handles file I/O, JSON parsing, and validation of data model constraints.
/// </summary>
public class DataService
{
    private const string DATA_FILE_PATH = "wwwroot/data/data.json";
    private const string MILESTONES_KEY = "milestones";
    private const string TASKS_KEY = "tasks";

    private readonly ILogger<DataService> _logger;

    public DataService(ILogger<DataService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Reads and deserializes project data from data.json asynchronously.
    /// Validates data model constraints and returns strongly-typed ProjectStatus object.
    /// </summary>
    /// <returns>Validated ProjectStatus object containing milestones and tasks.</returns>
    /// <exception cref="FileReadException">Thrown when data.json cannot be read from wwwroot/data.</exception>
    /// <exception cref="JsonParseException">Thrown when JSON is malformed or cannot be deserialized.</exception>
    /// <exception cref="ValidationException">Thrown when data model constraints are violated.</exception>
    /// <exception cref="DataLoadException">Thrown for unexpected errors during data loading.</exception>
    public async Task<ProjectStatus> ReadProjectDataAsync()
    {
        // Implementation in subsequent steps
        throw new NotImplementedException();
    }

    /// <summary>
    /// Loads JSON content from a file asynchronously.
    /// </summary>
    /// <param name="path">The file path to read from.</param>
    /// <returns>The file contents as a string.</returns>
    /// <exception cref="FileReadException">Thrown when file cannot be read.</exception>
    private async Task<string> LoadJsonFileAsync(string path)
    {
        try
        {
            _logger.LogInformation("Loading JSON from: {Path}", path);
            var json = await File.ReadAllTextAsync(path);
            _logger.LogInformation("Successfully loaded JSON file: {Path}", path);
            return json;
        }
        catch (FileNotFoundException)
        {
            var message = "data.json not found in wwwroot/data directory";
            _logger.LogError("File not found: {Path}. Message: {Message}", path, message);
            throw new FileReadException(message);
        }
        catch (IOException ex)
        {
            var message = $"Cannot read data.json: {ex.Message}";
            _logger.LogError(ex, "IO error reading file: {Path}. Message: {Message}", path, message);
            throw new FileReadException(message, ex);
        }
    }

    /// <summary>
    /// Deserializes JSON content into a ProjectStatus object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized ProjectStatus object.</returns>
    /// <exception cref="JsonParseException">Thrown when JSON is malformed or cannot be deserialized.</exception>
    private async Task<ProjectStatus> DeserializeProjectStatusAsync(string json)
    {
        try
        {
            _logger.LogInformation("Deserializing JSON content");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var projectStatus = JsonSerializer.Deserialize<ProjectStatus>(json, options);

            if (projectStatus == null)
            {
                throw new JsonParseException("Invalid JSON format in data.json: Deserialization resulted in null");
            }

            _logger.LogInformation("Successfully deserialized JSON content");
            return projectStatus;
        }
        catch (JsonException ex)
        {
            var message = $"Invalid JSON format in data.json: {ex.Message}";
            _logger.LogError(ex, "JSON parsing error: {Message}", message);
            throw new JsonParseException(message, ex);
        }
        catch (JsonParseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var message = $"Invalid JSON format in data.json: {ex.Message}";
            _logger.LogError(ex, "Unexpected error during JSON deserialization: {Message}", message);
            throw new JsonParseException(message, ex);
        }
    }
}