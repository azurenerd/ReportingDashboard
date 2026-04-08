using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Validates required fields, enum values, and data ranges per architecture specification.
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
        /// Validates project data structure, required fields, enum values, and data ranges.
        /// Per architecture specification: CompletionPercentage 0-100, DateTime fields ISO 8601 compliant,
        /// all enum values valid, milestones individually validated, work items enum values validated.
        /// </summary>
        private void ValidateProjectData(Project project)
        {
            if (project == null)
                throw new InvalidOperationException("Project data is null");

            if (string.IsNullOrEmpty(project.Name))
                throw new InvalidOperationException("Project name is required");

            if (project.Milestones == null || project.Milestones.Count == 0)
                throw new InvalidOperationException("At least one milestone is required");

            // Validate CompletionPercentage range (0-100)
            if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
                throw new InvalidOperationException(
                    $"CompletionPercentage must be between 0 and 100, got {project.CompletionPercentage}");

            // Validate project-level HealthStatus enum value
            if (!Enum.IsDefined(typeof(HealthStatus), project.HealthStatus))
                throw new InvalidOperationException(
                    $"Invalid HealthStatus value: {project.HealthStatus}");

            // Validate DateTime fields are ISO 8601 compliant (non-default DateTime)
            if (project.StartDate == default(DateTime))
                throw new InvalidOperationException(
                    "StartDate must be a valid ISO 8601 date (cannot be default/null)");

            if (project.TargetEndDate == default(DateTime))
                throw new InvalidOperationException(
                    "TargetEndDate must be a valid ISO 8601 date (cannot be default/null)");

            if (project.StartDate > project.TargetEndDate)
                throw new InvalidOperationException(
                    "StartDate must be before or equal to TargetEndDate");

            // Validate each milestone individually (per architecture requirement)
            ValidateMilestones(project.Milestones);

            // Validate work items enum values
            ValidateWorkItems(project.WorkItems);

            // Initialize empty work items list if null
            if (project.WorkItems == null)
                project.WorkItems = new List<WorkItem>();
        }

        /// <summary>
        /// Validates individual milestone objects for valid enum values and required fields.
        /// </summary>
        private void ValidateMilestones(List<Milestone> milestones)
        {
            if (milestones == null || milestones.Count == 0)
                throw new InvalidOperationException("Milestones collection cannot be empty");

            for (int i = 0; i < milestones.Count; i++)
            {
                var milestone = milestones[i];

                if (milestone == null)
                    throw new InvalidOperationException($"Milestone at index {i} is null");

                if (string.IsNullOrEmpty(milestone.Name))
                    throw new InvalidOperationException(
                        $"Milestone at index {i} has empty or null Name field (required)");

                if (milestone.TargetDate == default(DateTime))
                    throw new InvalidOperationException(
                        $"Milestone '{milestone.Name}' at index {i} has invalid TargetDate (cannot be default/null)");

                if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
                    throw new InvalidOperationException(
                        $"Milestone '{milestone.Name}' at index {i} has invalid Status enum value: {milestone.Status}");

                _logger.LogDebug("Milestone validated: {Name} (Status={Status})", milestone.Name, milestone.Status);
            }
        }

        /// <summary>
        /// Validates work item enum values to ensure all Status fields are valid.
        /// </summary>
        private void ValidateWorkItems(List<WorkItem> workItems)
        {
            if (workItems == null)
            {
                _logger.LogInformation("WorkItems collection is null, will be initialized as empty");
                return;
            }

            for (int i = 0; i < workItems.Count; i++)
            {
                var item = workItems[i];

                if (item == null)
                    throw new InvalidOperationException($"WorkItem at index {i} is null");

                if (string.IsNullOrEmpty(item.Title))
                    throw new InvalidOperationException(
                        $"WorkItem at index {i} has empty or null Title field (required)");

                if (!Enum.IsDefined(typeof(WorkItemStatus), item.Status))
                    throw new InvalidOperationException(
                        $"WorkItem '{item.Title}' at index {i} has invalid Status enum value: {item.Status}");

                _logger.LogDebug("WorkItem validated: {Title} (Status={Status})", item.Title, item.Status);
            }
        }
    }
}