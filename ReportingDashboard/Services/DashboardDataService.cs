using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<(DashboardData? Data, string? Error)> LoadAsync(string filename = "data.json")
    {
        if (string.IsNullOrWhiteSpace(filename) ||
            filename.Contains("..") ||
            filename.Contains('/') ||
            filename.Contains('\\') ||
            filename != Path.GetFileName(filename))
        {
            return (null, $"Invalid filename: {filename}");
        }

        var filePath = Path.Combine(_env.WebRootPath, filename);

        if (!File.Exists(filePath))
        {
            return (null, $"Could not load data file: {filename}");
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            return data is null
                ? (null, "data.json deserialized to null. Check file contents.")
                : (data, null);
        }
        catch (JsonException ex)
        {
            return (null, $"Error loading dashboard data: {ex.Message}. Please check {filename}.");
        }
        catch (IOException ex)
        {
            return (null, $"Error reading file: {ex.Message}");
        }
    }
}