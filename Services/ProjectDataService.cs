using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Models;

namespace AgentSquad.Services
{
    public class ProjectDataService
    {
        private readonly string _dataFilePath;

        public ProjectDataService()
        {
            _dataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "data.json");
        }

        public async Task<ProjectData> LoadProjectDataAsync()
        {
            if (!File.Exists(_dataFilePath))
            {
                throw new FileNotFoundException($"Data file not found at {_dataFilePath}");
            }

            string jsonContent = await File.ReadAllTextAsync(_dataFilePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ProjectData data = JsonSerializer.Deserialize<ProjectData>(jsonContent, options);

            if (data == null)
            {
                throw new InvalidOperationException("Failed to deserialize project data from JSON.");
            }

            CalculateMetrics(data);

            return data;
        }

        private void CalculateMetrics(ProjectData data)
        {
            if (data.Project == null || data.Tasks == null)
            {
                data.Metrics = new ProjectMetrics();
                return;
            }

            int totalTasks = data.Tasks.Count;
            int completedTasks = data.Tasks.Count(t => t.Status == TaskStatus.Shipped);
            int inProgressTasks = data.Tasks.Count(t => t.Status == TaskStatus.InProgress);
            int carriedOverTasks = data.Tasks.Count(t => t.Status == TaskStatus.CarriedOver);

            DateTime today = DateTime.UtcNow;
            DateTime projectEnd = data.Project.EndDate;
            int daysRemaining = Math.Max(0, (int)(projectEnd - today).TotalDays);

            double estimatedBurndownRate = totalTasks > 0 ? (completedTasks / (double)totalTasks) * 100 : 0;

            data.Metrics = new ProjectMetrics
            {
                CompletionPercentage = data.Project.CompletionPercentage,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                CarriedOverTasks = carriedOverTasks,
                ProjectStartDate = data.Project.StartDate,
                ProjectEndDate = projectEnd,
                DaysRemaining = daysRemaining,
                EstimatedBurndownRate = Math.Round(estimatedBurndownRate, 2)
            };
        }
    }
}