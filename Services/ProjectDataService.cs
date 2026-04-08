using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Services
{
    public class ProjectDataService
    {
        private readonly string _dataPath;

        public ProjectDataService(IConfiguration configuration)
        {
            _dataPath = configuration["DataFilePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");
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
                throw new ArgumentException("Project name is required.");
            }

            if (data.Milestones == null)
            {
                throw new ArgumentException("Milestones list is required.");
            }

            if (data.Tasks == null)
            {
                throw new ArgumentException("Tasks list is required.");
            }

            if (data.Metrics == null)
            {
                throw new ArgumentException("Metrics object is required.");
            }
        }
    }
}