using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AgentSquad.Runner.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
    public class DataProvider : IDataProvider
    {
        private readonly ILogger<DataProvider> _logger;
        private readonly IDataCache _dataCache;
        private readonly IWebHostEnvironment _hostEnvironment;
        private const string CacheKey = "project_data";

        public DataProvider(ILogger<DataProvider> logger, IDataCache dataCache, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _dataCache = dataCache;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<Project> LoadProjectDataAsync()
        {
            try
            {
                if (_dataCache.TryGetValue<Project>(CacheKey, out var cachedProject))
                {
                    _logger.LogInformation("Retrieved project data from cache");
                    return cachedProject;
                }

                string dataFilePath = Path.Combine(_hostEnvironment.ContentRootPath, "data.json");

                if (!File.Exists(dataFilePath))
                {
                    _logger.LogError($"data.json not found at: {dataFilePath}");
                    throw new FileNotFoundException($"Data file not found: {dataFilePath}");
                }

                string jsonContent = await File.ReadAllTextAsync(dataFilePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                var project = JsonSerializer.Deserialize<Project>(jsonContent, options);

                if (project == null)
                {
                    _logger.LogError("Failed to deserialize data.json: result was null");
                    throw new InvalidOperationException("Deserialization resulted in null project");
                }

                ValidateProjectData(project);

                _dataCache.Set(CacheKey, project);

                _logger.LogInformation(
                    $"Successfully loaded project data: {project.Name} " +
                    $"with {project.Milestones.Count} milestones and " +
                    $"{project.WorkItems.Count} work items");

                return project;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON parsing error in data.json: {ex.Message}");
                throw new InvalidOperationException("Invalid JSON format in data.json", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading project data: {ex.Message}");
                throw;
            }
        }

        public Project GetCachedProjectData()
        {
            if (_dataCache.TryGetValue<Project>(CacheKey, out var cachedProject))
            {
                return cachedProject;
            }
            _logger.LogWarning("Attempted to access cached project data before loading");
            return null;
        }

        private void ValidateProjectData(Project project)
        {
            var errors = new System.Collections.Generic.List<string>();

            if (string.IsNullOrWhiteSpace(project.Name))
                errors.Add("Project name is required");

            if (project.Milestones == null || project.Milestones.Count < 1)
                errors.Add("At least 1 milestone is required");

            if (project.WorkItems == null)
                errors.Add("Work items collection is required");

            if (project.Metrics == null)
                errors.Add("Project metrics are required");

            if (project.Metrics != null)
            {
                if (project.Metrics.CompletionPercentage < 0 || project.Metrics.CompletionPercentage > 100)
                    errors.Add("CompletionPercentage must be between 0 and 100");

                if (project.Metrics.VelocityCount < 0)
                    errors.Add("VelocityCount must be non-negative");
            }

            if (errors.Count > 0)
            {
                string errorMessage = string.Join("; ", errors);
                _logger.LogError($"Data validation errors: {errorMessage}");
                throw new InvalidOperationException($"Data validation failed: {errorMessage}");
            }
        }
    }
}