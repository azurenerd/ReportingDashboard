using System;
using System.IO;
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

            return data;
        }
    }
}