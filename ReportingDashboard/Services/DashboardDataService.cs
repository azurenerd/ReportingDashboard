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

    public DashboardData? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool HasError => Data is null;

    public async Task LoadAsync()
    {
        try
        {
            var path = Path.Combine(_env.WebRootPath, "data", "data.json");
            var json = await File.ReadAllTextAsync(path);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var data = JsonSerializer.Deserialize<DashboardData>(json, options);

            if (data is null)
            {
                ErrorMessage = "data.json deserialized to null. Check that the file contains valid JSON.";
                return;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(data.Title))
            {
                ErrorMessage = "Missing required field 'title' in data.json.";
                return;
            }

            if (data.MilestoneStreams is null || data.MilestoneStreams.Count == 0)
            {
                ErrorMessage = "Missing or empty required field 'milestoneStreams' in data.json.";
                return;
            }

            if (data.Heatmap is null)
            {
                ErrorMessage = "Missing required field 'heatmap' in data.json.";
                return;
            }

            if (data.Heatmap.Columns is null || data.Heatmap.Columns.Count == 0)
            {
                ErrorMessage = "Missing or empty required field 'heatmap.columns' in data.json.";
                return;
            }

            if (data.Heatmap.Rows is null || data.Heatmap.Rows.Count == 0)
            {
                ErrorMessage = "Missing or empty required field 'heatmap.rows' in data.json.";
                return;
            }

            if (data.Heatmap.CurrentColumnIndex < 0 || data.Heatmap.CurrentColumnIndex >= data.Heatmap.Columns.Count)
            {
                ErrorMessage = $"'heatmap.currentColumnIndex' value {data.Heatmap.CurrentColumnIndex} is out of range. Must be 0-{data.Heatmap.Columns.Count - 1}.";
                return;
            }

            Data = data;
            ErrorMessage = null;
        }
        catch (FileNotFoundException)
        {
            ErrorMessage = "data.json not found. Place a valid data.json file in wwwroot/data/ and restart the application.";
        }
        catch (DirectoryNotFoundException)
        {
            ErrorMessage = "data.json not found. Place a valid data.json file in wwwroot/data/ and restart the application.";
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"data.json is malformed: {ex.Message}. Check JSON syntax and required fields.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error loading data.json: {ex.Message}";
        }
    }
}