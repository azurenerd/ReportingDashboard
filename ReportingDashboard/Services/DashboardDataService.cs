using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "Data", "data.json");
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public (DashboardData? Data, string? Error) Load()
    {
        try
        {
            if (!File.Exists(_path))
                return (null, $"data.json not found at: {_path}");

            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<DashboardData>(json, _options);
            
            if (data is null)
                return (null, "data.json deserialized to null.");

            var error = Validate(data);
            return (error is null ? data : null, error);
        }
        catch (JsonException ex)
        {
            return (null, $"Invalid JSON in data.json: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (null, $"Error loading data.json: {ex.Message}");
        }
    }

    private static string? Validate(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Project?.Title))
            return "Missing required field: project.title";
        if (string.IsNullOrWhiteSpace(data.Project?.Subtitle))
            return "Missing required field: project.subtitle";
        if (string.IsNullOrWhiteSpace(data.Project?.BacklogUrl))
            return "Missing required field: project.backlogUrl";
        if (string.IsNullOrWhiteSpace(data.Project?.CurrentMonth))
            return "Missing required field: project.currentMonth";
        if (data.Timeline?.Months is null || data.Timeline.Months.Count == 0)
            return "Missing required field: timeline.months";
        if (data.Timeline.NowPosition < 0.0 || data.Timeline.NowPosition > 1.0)
            return "timeline.nowPosition must be between 0.0 and 1.0";
        if (data.Tracks is null)
            return "Missing required field: tracks";
        if (data.Heatmap?.Categories is null)
            return "Missing required field: heatmap.categories";

        foreach (var track in data.Tracks)
        {
            if (track.Milestones is not null)
            {
                foreach (var milestone in track.Milestones)
                {
                    if (milestone.Position < 0.0 || milestone.Position > 1.0)
                        return $"Milestone position must be between 0.0 and 1.0 in track '{track.Id}'";
                    if (!new[] { "checkpoint", "poc", "production" }.Contains(milestone.Type?.ToLower() ?? ""))
                        return $"Invalid milestone type '{milestone.Type}' in track '{track.Id}'. Must be 'checkpoint', 'poc', or 'production'.";
                }
            }
        }

        foreach (var category in data.Heatmap.Categories)
        {
            if (!new[] { "ship", "prog", "carry", "block" }.Contains(category.CssClass?.ToLower() ?? ""))
                return $"Invalid cssClass '{category.CssClass}'. Must be 'ship', 'prog', 'carry', or 'block'.";
        }

        return null;
    }
}