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
        private readonly ProjectDataValidator _validator;

        public ProjectDataService(ProjectDataValidator validator)
        {
            _dataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "data.json");
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<(ProjectData Data, string Error)> LoadProjectDataAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return (null, $"Data file not found at {_dataFilePath}");
                }

                string jsonContent = await File.ReadAllTextAsync(_dataFilePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                ProjectData data = JsonSerializer.Deserialize<ProjectData>(jsonContent, options);

                if (data == null)
                {
                    return (null, "Failed to deserialize project data.");
                }

                var validationResult = _validator.Validate(data);
                if (!validationResult.IsValid)
                {
                    return (null, $"Data validation failed:\n{validationResult.GetErrorMessage()}");
                }

                return (data, null);
            }
            catch (JsonException ex)
            {
                return (null, $"Invalid JSON format: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, $"Error loading project data: {ex.Message}");
            }
        }
    }
}