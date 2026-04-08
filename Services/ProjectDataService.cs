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
                _logger.LogInformation($"Successfully loaded project data: {projectData.ProjectInfo?.ProjectName}");

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
            if (data.ProjectInfo == null)
                throw new InvalidOperationException("ProjectInfo is required");

            ValidateProjectInfo(data.ProjectInfo);

            if (data.ProjectMetrics == null)
                throw new InvalidOperationException("ProjectMetrics is required");

            ValidateProjectMetrics(data.ProjectMetrics);

            if (data.Milestones != null)
            {
                foreach (var milestone in data.Milestones)
                {
                    ValidateMilestone(milestone);
                }
            }

            if (data.Tasks != null)
            {
                foreach (var task in data.Tasks)
                {
                    ValidateTask(task);
                }
            }
        }

        private void ValidateProjectInfo(ProjectInfo projectInfo)
        {
            if (string.IsNullOrWhiteSpace(projectInfo.ProjectName))
                throw new InvalidOperationException("ProjectInfo.ProjectName is required");

            if (string.IsNullOrWhiteSpace(projectInfo.Sponsor))
                throw new InvalidOperationException("ProjectInfo.Sponsor is required");

            if (string.IsNullOrWhiteSpace(projectInfo.ProjectManager))
                throw new InvalidOperationException("ProjectInfo.ProjectManager is required");

            if (string.IsNullOrWhiteSpace(projectInfo.Status))
                throw new InvalidOperationException("ProjectInfo.Status is required");

            if (string.IsNullOrWhiteSpace(projectInfo.ProjectStartDate))
                throw new InvalidOperationException("ProjectInfo.ProjectStartDate is required");

            if (string.IsNullOrWhiteSpace(projectInfo.ProjectEndDate))
                throw new InvalidOperationException("ProjectInfo.ProjectEndDate is required");

            ValidateDateFormat(projectInfo.ProjectStartDate, "ProjectInfo.ProjectStartDate");
            ValidateDateFormat(projectInfo.ProjectEndDate, "ProjectInfo.ProjectEndDate");
        }

        private void ValidateProjectMetrics(ProjectMetrics metrics)
        {
            if (metrics.TotalTasks < 0)
                throw new InvalidOperationException("ProjectMetrics.TotalTasks must be non-negative");

            if (metrics.OverallCompletionPercentage < 0 || metrics.OverallCompletionPercentage > 100)
                throw new InvalidOperationException("ProjectMetrics.OverallCompletionPercentage must be between 0 and 100");
        }

        private void ValidateMilestone(Milestone milestone)
        {
            if (string.IsNullOrWhiteSpace(milestone.Id))
                throw new InvalidOperationException("Milestone.Id is required");

            if (string.IsNullOrWhiteSpace(milestone.Name))
                throw new InvalidOperationException("Milestone.Name is required");

            if (string.IsNullOrWhiteSpace(milestone.TargetDate))
                throw new InvalidOperationException("Milestone.TargetDate is required");

            ValidateDateFormat(milestone.TargetDate, "Milestone.TargetDate");

            if (!string.IsNullOrWhiteSpace(milestone.ActualDate))
            {
                ValidateDateFormat(milestone.ActualDate, "Milestone.ActualDate");
            }

            if (milestone.CompletionPercentage < 0 || milestone.CompletionPercentage > 100)
                throw new InvalidOperationException("Milestone.CompletionPercentage must be between 0 and 100");
        }

        private void ValidateTask(ProjectTask task)
        {
            if (string.IsNullOrWhiteSpace(task.Id))
                throw new InvalidOperationException("Task.Id is required");

            if (string.IsNullOrWhiteSpace(task.Name))
                throw new InvalidOperationException("Task.Name is required");

            if (string.IsNullOrWhiteSpace(task.Status))
                throw new InvalidOperationException("Task.Status is required");

            if (string.IsNullOrWhiteSpace(task.AssignedTo))
                throw new InvalidOperationException("Task.AssignedTo is required");

            if (string.IsNullOrWhiteSpace(task.DueDate))
                throw new InvalidOperationException("Task.DueDate is required");

            ValidateDateFormat(task.DueDate, "Task.DueDate");

            if (task.EstimatedDays <= 0)
                throw new InvalidOperationException("Task.EstimatedDays must be positive");
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