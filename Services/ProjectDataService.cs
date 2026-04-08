using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Dashboard.Models;
using Microsoft.Extensions.Configuration;

namespace AgentSquad.Dashboard.Services
{
    public class ProjectDataService
    {
        private readonly string _dataPath;

        public ProjectDataService(IConfiguration configuration)
        {
            var configuredPath = configuration["DataFilePath"];
            
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                _dataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");
            }
            else if (Path.IsPathRooted(configuredPath))
            {
                _dataPath = configuredPath;
            }
            else
            {
                _dataPath = Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
            }
        }

        public async Task<ProjectData> LoadProjectDataAsync()
        {
            if (!File.Exists(_dataPath))
            {
                throw new FileNotFoundException($"Data file not found at: {_dataPath}");
            }

            var jsonContent = await File.ReadAllTextAsync(_dataPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                var projectData = JsonSerializer.Deserialize<ProjectData>(jsonContent, options);
                
                if (projectData == null)
                {
                    throw new ArgumentException("Deserialized project data is null. Check data.json structure.");
                }

                ValidateProjectData(projectData);
                return projectData;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to parse JSON: {ex.Message}", ex);
            }
        }

        private void ValidateProjectData(ProjectData data)
        {
            if (string.IsNullOrWhiteSpace(data.ProjectName))
            {
                throw new ArgumentException("Project name is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(data.Sponsor))
            {
                throw new ArgumentException("Sponsor name is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(data.ProjectManager))
            {
                throw new ArgumentException("Project manager name is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(data.Status))
            {
                throw new ArgumentException("Project status is required and cannot be empty.");
            }

            if (data.ProjectStartDate == default(DateTime) || data.ProjectEndDate == default(DateTime))
            {
                throw new ArgumentException("Project start date and end date must be valid dates.");
            }

            if (data.ProjectStartDate >= data.ProjectEndDate)
            {
                throw new ArgumentException("Project start date must be before project end date.");
            }

            if (data.Milestones == null)
            {
                throw new ArgumentException("Milestones list is required.");
            }

            foreach (var milestone in data.Milestones)
            {
                if (string.IsNullOrWhiteSpace(milestone.Name))
                {
                    throw new ArgumentException("Milestone name is required and cannot be empty.");
                }

                if (milestone.TargetDate == default(DateTime))
                {
                    throw new ArgumentException($"Milestone '{milestone.Name}' target date must be a valid date.");
                }

                if (milestone.TargetDate < data.ProjectStartDate || milestone.TargetDate > data.ProjectEndDate)
                {
                    throw new ArgumentException($"Milestone '{milestone.Name}' target date must be within project date range.");
                }

                if (milestone.CompletionPercentage < 0 || milestone.CompletionPercentage > 100)
                {
                    throw new ArgumentException($"Milestone '{milestone.Name}' completion percentage must be between 0 and 100.");
                }

                if (string.IsNullOrWhiteSpace(milestone.Status))
                {
                    throw new ArgumentException($"Milestone '{milestone.Name}' status is required.");
                }
            }

            if (data.Tasks == null)
            {
                throw new ArgumentException("Tasks list is required.");
            }

            foreach (var task in data.Tasks)
            {
                if (string.IsNullOrWhiteSpace(task.Name))
                {
                    throw new ArgumentException("Task name is required and cannot be empty.");
                }

                if (string.IsNullOrWhiteSpace(task.Owner))
                {
                    throw new ArgumentException($"Task '{task.Name}' owner is required and cannot be empty.");
                }
            }

            if (data.Metrics == null)
            {
                throw new ArgumentException("Metrics object is required.");
            }

            if (data.Metrics.CompletionPercentage < 0 || data.Metrics.CompletionPercentage > 100)
            {
                throw new ArgumentException("Metrics completion percentage must be between 0 and 100.");
            }

            if (data.Metrics.TotalTasks < 0 || data.Metrics.TasksCompleted < 0 || 
                data.Metrics.TasksInProgress < 0 || data.Metrics.TasksCarriedOver < 0)
            {
                throw new ArgumentException("Metrics task counts must be non-negative.");
            }

            int totalTasksInMetrics = data.Metrics.TasksCompleted + data.Metrics.TasksInProgress + data.Metrics.TasksCarriedOver;
            if (totalTasksInMetrics != data.Metrics.TotalTasks)
            {
                throw new ArgumentException("Sum of task statuses must equal total tasks.");
            }
        }
    }
}