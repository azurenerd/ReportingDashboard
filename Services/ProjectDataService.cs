using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
    public class ProjectDataService
    {
        private readonly ILogger<ProjectDataService> _logger;
        private readonly string _dataFilePath;

        public ProjectDataService(ILogger<ProjectDataService> logger)
        {
            _logger = logger;
            _dataFilePath = Path.Combine("wwwroot", "data", "data.json");
        }

        public async Task<ProjectData> LoadProjectDataAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    _logger.LogError($"Data file not found at {_dataFilePath}");
                    throw new FileNotFoundException($"Data file not found: {_dataFilePath}");
                }

                var json = await File.ReadAllTextAsync(_dataFilePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var projectData = JsonSerializer.Deserialize<ProjectData>(json, options);

                if (projectData == null)
                {
                    throw new InvalidOperationException("Failed to deserialize project data");
                }

                ValidateProjectData(projectData);
                _logger.LogInformation($"Successfully loaded project data: {projectData.ProjectName}");

                return projectData;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON parsing error: {ex.Message}");
                throw new InvalidOperationException("Invalid JSON format in data.json", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading project data: {ex.Message}");
                throw;
            }
        }

        private void ValidateProjectData(ProjectData data)
        {
            if (string.IsNullOrWhiteSpace(data.ProjectName))
                throw new InvalidOperationException("ProjectName is required");

            if (string.IsNullOrWhiteSpace(data.ProjectStartDate))
                throw new InvalidOperationException("ProjectStartDate is required");

            if (string.IsNullOrWhiteSpace(data.ProjectEndDate))
                throw new InvalidOperationException("ProjectEndDate is required");

            ValidateDateFormat(data.ProjectStartDate, nameof(data.ProjectStartDate));
            ValidateDateFormat(data.ProjectEndDate, nameof(data.ProjectEndDate));

            if (data.Milestones != null)
            {
                foreach (var milestone in data.Milestones)
                {
                    if (string.IsNullOrWhiteSpace(milestone.Name))
                        throw new InvalidOperationException("Milestone name is required");
                    ValidateDateFormat(milestone.TargetDate, "Milestone.TargetDate");
                }
            }

            if (data.Tasks != null)
            {
                foreach (var task in data.Tasks)
                {
                    if (string.IsNullOrWhiteSpace(task.Name))
                        throw new InvalidOperationException("Task name is required");
                    if (string.IsNullOrWhiteSpace(task.Status))
                        throw new InvalidOperationException("Task status is required");
                }
            }
        }

        private void ValidateDateFormat(string dateString, string fieldName)
        {
            if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, 
                System.Globalization.DateTimeStyles.None, out _))
            {
                throw new InvalidOperationException($"{fieldName} must be in ISO 8601 format (YYYY-MM-DD): {dateString}");
            }
        }
    }
}