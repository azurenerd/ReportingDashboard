using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.AspNetCore.Hosting;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service for loading, parsing, validating, and caching project data from data.json.
    /// </summary>
    public class DataProvider : IDataProvider
    {
        private readonly IDataCache _cache;
        private readonly ILogger<DataProvider> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string DATA_FILE_NAME = "data.json";
        private const string CACHE_KEY = "project_data";
        private const int DEFAULT_CACHE_TTL_HOURS = 1;

        /// <summary>
        /// Initializes a new instance of the DataProvider class.
        /// </summary>
        /// <param name="cache">The data cache service.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="webHostEnvironment">The web host environment for accessing wwwroot.</param>
        public DataProvider(
            IDataCache cache,
            ILogger<DataProvider> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        /// <summary>
        /// Loads project data from data.json file, using cache when available.
        /// </summary>
        /// <returns>The loaded and validated Project data.</returns>
        /// <exception cref="FileNotFoundException">Thrown when data.json file is not found.</exception>
        /// <exception cref="JsonException">Thrown when JSON is invalid or malformed.</exception>
        /// <exception cref="InvalidOperationException">Thrown when data validation fails.</exception>
        public async Task<Project> LoadProjectDataAsync()
        {
            try
            {
                // Check cache first
                var cachedProject = await _cache.GetAsync<Project>(CACHE_KEY);
                if (cachedProject != null)
                {
                    _logger.LogInformation("Returning project data from cache");
                    return cachedProject;
                }

                _logger.LogInformation("Cache miss, loading project data from file");

                // Construct file path
                var dataFilePath = Path.Combine(_webHostEnvironment.WebRootPath, DATA_FILE_NAME);

                // Check if file exists
                if (!File.Exists(dataFilePath))
                {
                    var errorMessage = $"Configuration file not found at '{dataFilePath}'. Please create '{DATA_FILE_NAME}' in the wwwroot folder with valid project data.";
                    _logger.LogError("File not found: {FilePath}", dataFilePath);
                    throw new FileNotFoundException(errorMessage, dataFilePath);
                }

                // Read file asynchronously
                var jsonContent = await File.ReadAllTextAsync(dataFilePath);

                // Configure JSON deserialization options
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                // Deserialize JSON to Project model
                Project project;
                try
                {
                    project = JsonSerializer.Deserialize<Project>(jsonContent, options);
                }
                catch (JsonException jsonEx)
                {
                    var errorMessage = $"Invalid JSON format in '{DATA_FILE_NAME}': {jsonEx.Message}";
                    _logger.LogError(jsonEx, "JSON deserialization failed: {ErrorMessage}", errorMessage);
                    throw new JsonException(errorMessage, jsonEx);
                }

                // Validate project data
                ValidateProjectData(project);

                // Cache the result
                await _cache.SetAsync(CACHE_KEY, project, TimeSpan.FromHours(DEFAULT_CACHE_TTL_HOURS));
                _logger.LogInformation("Project data loaded and cached successfully");

                return project;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (JsonException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading project data");
                throw new InvalidOperationException($"Failed to load project data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Invalidates the cached project data, forcing a reload from file on next call.
        /// </summary>
        public void InvalidateCache()
        {
            _cache.Remove(CACHE_KEY);
            _logger.LogInformation("Project data cache invalidated");
        }

        /// <summary>
        /// Validates that project data meets all required constraints.
        /// </summary>
        /// <param name="project">The project to validate.</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        private void ValidateProjectData(Project project)
        {
            if (project == null)
            {
                const string message = "Project data is null. Ensure data.json contains valid project configuration.";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            if (string.IsNullOrWhiteSpace(project.Name))
            {
                const string message = "Project name is required. Ensure 'name' field is present and non-empty in data.json.";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            if (project.Milestones == null || project.Milestones.Count == 0)
            {
                const string message = "At least one milestone is required. Ensure 'milestones' array contains at least one item in data.json.";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            if (project.WorkItems == null)
            {
                _logger.LogWarning("WorkItems array is null; initializing empty list");
                project.WorkItems = new List<WorkItem>();
            }

            if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
            {
                var message = $"Completion percentage must be between 0 and 100, but was {project.CompletionPercentage}.";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            // Validate milestone data
            foreach (var milestone in project.Milestones)
            {
                if (string.IsNullOrWhiteSpace(milestone.Name))
                {
                    const string message = "Milestone name cannot be empty. All milestones must have a 'name' field.";
                    _logger.LogError(message);
                    throw new InvalidOperationException(message);
                }

                if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
                {
                    var message = $"Invalid milestone status '{milestone.Status}'. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MilestoneStatus)))}.";
                    _logger.LogError(message);
                    throw new InvalidOperationException(message);
                }
            }

            // Validate work item data
            foreach (var workItem in project.WorkItems)
            {
                if (string.IsNullOrWhiteSpace(workItem.Title))
                {
                    const string message = "Work item title cannot be empty. All work items must have a 'title' field.";
                    _logger.LogError(message);
                    throw new InvalidOperationException(message);
                }

                if (!Enum.IsDefined(typeof(WorkItemStatus), workItem.Status))
                {
                    var message = $"Invalid work item status '{workItem.Status}'. Valid values are: {string.Join(", ", Enum.GetNames(typeof(WorkItemStatus)))}.";
                    _logger.LogError(message);
                    throw new InvalidOperationException(message);
                }
            }

            // Validate health status
            if (!Enum.IsDefined(typeof(HealthStatus), project.HealthStatus))
            {
                var message = $"Invalid health status '{project.HealthStatus}'. Valid values are: {string.Join(", ", Enum.GetNames(typeof(HealthStatus)))}.";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            _logger.LogDebug("Project data validation passed");
        }
    }
}