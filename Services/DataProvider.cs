using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AgentSquad.Models;

namespace AgentSquad.Services
{
    /// <summary>
    /// Service for loading, parsing, validating, and caching project data from data.json.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Loads project data asynchronously from data.json via cache or file system.
        /// </summary>
        /// <returns>Strongly-typed Project object</returns>
        /// <exception cref="FileNotFoundException">data.json not found</exception>
        /// <exception cref="JsonException">Invalid JSON syntax</exception>
        /// <exception cref="InvalidOperationException">Validation failure or missing required fields</exception>
        Task<Project> LoadProjectDataAsync();

        /// <summary>
        /// Invalidates cached project data, forcing reload on next request.
        /// </summary>
        void InvalidateCache();
    }

    /// <summary>
    /// Implementation of IDataProvider using async cache and System.Text.Json deserialization.
    /// </summary>
    public class DataProvider : IDataProvider
    {
        private readonly IDataCache _cache;
        private readonly ILogger<DataProvider> _logger;
        private const string DATA_FILE_PATH = "wwwroot/data.json";
        private const string CACHE_KEY = "project_data";
        private static readonly TimeSpan CacheTTL = TimeSpan.FromHours(1);

        public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads project data from cache or file system with async pattern.
        /// </summary>
        public async Task<Project> LoadProjectDataAsync()
        {
            try
            {
                // Check cache first
                var cached = await _cache.GetAsync<Project>(CACHE_KEY);
                if (cached != null)
                {
                    _logger.LogInformation("Project data loaded from cache");
                    return cached;
                }

                // Read and parse JSON with case-insensitive enum support
                var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };

                var project = JsonSerializer.Deserialize<Project>(json, options);

                // Validate structure
                ValidateProjectData(project);

                // Cache result
                await _cache.SetAsync(CACHE_KEY, project, CacheTTL);

                _logger.LogInformation("Project data loaded from file and cached for 1 hour");
                return project;
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "data.json not found at {Path}", DATA_FILE_PATH);
                throw new InvalidOperationException(
                    $"Configuration file not found at {DATA_FILE_PATH}. Please ensure data.json exists in the wwwroot directory.",
                    ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed");
                throw new InvalidOperationException(
                    "Invalid JSON format in data.json. Please check file syntax and schema.",
                    ex);
            }
        }

        /// <summary>
        /// Invalidates cache, forcing reload on next LoadProjectDataAsync call.
        /// </summary>
        public void InvalidateCache()
        {
            _cache.Remove(CACHE_KEY);
            _logger.LogInformation("Project data cache invalidated");
        }

        /// <summary>
        /// Validates project data structure and required fields.
        /// </summary>
        private static void ValidateProjectData(Project project)
        {
            if (project == null)
                throw new InvalidOperationException("Project data is null");

            if (string.IsNullOrEmpty(project.Name))
                throw new InvalidOperationException("Project name is required");

            if (project.Milestones == null || project.Milestones.Count == 0)
                throw new InvalidOperationException("At least one milestone is required");

            // Initialize empty work items list if null
            if (project.WorkItems == null)
                project.WorkItems = new List<WorkItem>();

            // Validate enum values are in range
            if (!Enum.IsDefined(typeof(HealthStatus), project.HealthStatus))
                throw new InvalidOperationException(
                    $"Invalid HealthStatus value: {project.HealthStatus}");
        }
    }
}