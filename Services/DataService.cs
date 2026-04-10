using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class DataService : IDataService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly JsonSerializerOptions _jsonOptions;

        public DataService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
        }

        public async System.Threading.Tasks.Task<ProjectStatus> ReadProjectDataAsync()
        {
            var dataPath = Path.Combine(_environment.WebRootPath, "data", "data.json");

            if (!File.Exists(dataPath))
            {
                throw new FileNotFoundException($"Data file not found at {dataPath}");
            }

            try
            {
                var jsonContent = await File.ReadAllTextAsync(dataPath);
                var projectStatus = JsonSerializer.Deserialize<ProjectStatus>(jsonContent, _jsonOptions);

                if (projectStatus == null)
                {
                    throw new JsonException("Deserialized project status is null");
                }

                projectStatus.LastUpdated = DateTime.UtcNow;
                ValidateProjectStatus(projectStatus);

                return projectStatus;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to parse data.json: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading project data: {ex.Message}", ex);
            }
        }

        private void ValidateProjectStatus(ProjectStatus projectStatus)
        {
            if (projectStatus.Milestones == null)
            {
                projectStatus.Milestones = new List<Milestone>();
            }

            if (projectStatus.Tasks == null)
            {
                projectStatus.Tasks = new List<Models.Task>();
            }

            foreach (var milestone in projectStatus.Milestones)
            {
                if (string.IsNullOrWhiteSpace(milestone.Name))
                {
                    throw new JsonException("Milestone must have a non-empty name");
                }

                if (milestone.TargetDate == default)
                {
                    throw new JsonException($"Milestone '{milestone.Name}' has an invalid target date");
                }
            }

            foreach (var task in projectStatus.Tasks)
            {
                if (string.IsNullOrWhiteSpace(task.Title))
                {
                    throw new JsonException("Task must have a non-empty title");
                }
            }
        }
    }
}