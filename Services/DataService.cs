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

    /// <summary>
    /// Validates project status data model constraints.
    /// Checks for unique IDs, valid enum values, and proper date formats.
    /// </summary>
    /// <param name="data">The ProjectStatus object to validate.</param>
    /// <exception cref="ValidationException">Thrown when validation constraints are violated.</exception>
    private void ValidateProjectStatus(ProjectStatus data)
    {
        try
        {
            _logger.LogInformation("Starting ProjectStatus validation");

            if (data == null)
            {
                throw new ValidationException("ProjectStatus cannot be null");
            }

            // Validate milestone uniqueness
            var milestoneIds = new HashSet<string>();
            if (data.Milestones != null)
            {
                foreach (var milestone in data.Milestones)
                {
                    if (string.IsNullOrWhiteSpace(milestone.Id))
                    {
                        throw new ValidationException("Milestone ID cannot be empty");
                    }

                    if (!milestoneIds.Add(milestone.Id))
                    {
                        throw new ValidationException($"Duplicate milestone ID: {milestone.Id}");
                    }
                }
                _logger.LogInformation("Milestone ID uniqueness validation passed: {Count} milestones", milestoneIds.Count);
            }

            // Validate task uniqueness
            var taskIds = new HashSet<string>();
            if (data.Tasks != null)
            {
                foreach (var task in data.Tasks)
                {
                    if (string.IsNullOrWhiteSpace(task.Id))
                    {
                        throw new ValidationException("Task ID cannot be empty");
                    }

                    if (!taskIds.Add(task.Id))
                    {
                        throw new ValidationException($"Duplicate task ID: {task.Id}");
                    }
                }
                _logger.LogInformation("Task ID uniqueness validation passed: {Count} tasks", taskIds.Count);
            }

            // Validate enum values for tasks
            if (data.Tasks != null)
            {
                foreach (var task in data.Tasks)
                {
                    var validTaskStatuses = new[] { "Completed", "InProgress", "CarriedOver" };
                    var statusStr = task.Status.ToString();

                    if (!validTaskStatuses.Contains(statusStr))
                    {
                        throw new ValidationException($"Invalid task status: {statusStr}. Must be one of: Completed, InProgress, CarriedOver");
                    }
                }
                _logger.LogInformation("Task status enum validation passed");
            }

            // Validate enum values for milestones
            if (data.Milestones != null)
            {
                foreach (var milestone in data.Milestones)
                {
                    var validMilestoneStatuses = new[] { "OnTrack", "AtRisk", "Completed" };
                    var statusStr = milestone.Status.ToString();

                    if (!validMilestoneStatuses.Contains(statusStr))
                    {
                        throw new ValidationException($"Invalid milestone status: {statusStr}. Must be one of: OnTrack, AtRisk, Completed");
                    }
                }
                _logger.LogInformation("Milestone status enum validation passed");
            }

            // Validate dates
            if (data.Tasks != null)
            {
                foreach (var task in data.Tasks)
                {
                    if (task.DueDate == default(DateTime))
                    {
                        throw new ValidationException($"Task {task.Id} has invalid DueDate: cannot be default/empty");
                    }

                    if (task.DueDate.Kind != DateTimeKind.Utc && task.DueDate.Kind != DateTimeKind.Unspecified)
                    {
                        _logger.LogWarning("Task {TaskId} DueDate may not be in UTC format", task.Id);
                    }
                }
                _logger.LogInformation("Task date validation passed");
            }

            if (data.Milestones != null)
            {
                foreach (var milestone in data.Milestones)
                {
                    if (milestone.TargetDate == default(DateTime))
                    {
                        throw new ValidationException($"Milestone {milestone.Id} has invalid TargetDate: cannot be default/empty");
                    }

                    if (milestone.TargetDate.Kind != DateTimeKind.Utc && milestone.TargetDate.Kind != DateTimeKind.Unspecified)
                    {
                        _logger.LogWarning("Milestone {MilestoneId} TargetDate may not be in UTC format", milestone.Id);
                    }
                }
                _logger.LogInformation("Milestone date validation passed");
            }

            _logger.LogInformation("ProjectStatus validation completed successfully");
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var message = $"Unexpected error during validation: {ex.Message}";
            _logger.LogError(ex, "Validation error: {Message}", message);
            throw new ValidationException(message, ex);
        }
    }
}