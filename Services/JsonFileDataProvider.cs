using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class JsonFileDataProvider : IDataProvider
{
    private readonly string _filePath;

    public JsonFileDataProvider(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<ProjectData> LoadAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Data file not found: {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<ProjectData>(json, options)
                ?? throw new InvalidOperationException("Failed to deserialize project data");

            var context = new ValidationContext(data);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(data, context, results, validateAllProperties: true))
            {
                var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            return data;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON: {ex.Message}", ex);
        }
    }
}