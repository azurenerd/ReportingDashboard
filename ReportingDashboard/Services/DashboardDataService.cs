using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    private static readonly HashSet<string> ValidCssClasses = new() { "ship", "prog", "carry", "block" };
    private static readonly HashSet<string> ValidMilestoneTypes = new() { "checkpoint", "poc", "production" };

    public DashboardDataService(IWebHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "Data", "data.json");
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
            return error is null ? (data, null) : (null, error);
        }
        catch (JsonException ex)
        {
            return (null, $"Invalid JSON in data.json: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (null, $"Error reading data.json: {ex.Message}");
        }
    }

    private static string? Validate(DashboardData data)
    {
        // Project validation
        if (data.Project is null)
            return "Missing required field: project";
        if (string.IsNullOrWhiteSpace(data.Project.Title))
            return "Missing required field: project.title";
        if (string.IsNullOrWhiteSpace(data.Project.Subtitle))
            return "Missing required field: project.subtitle";
        if (string.IsNullOrWhiteSpace(data.Project.CurrentMonth))
            return "Missing required field: project.currentMonth";

        // Timeline validation
        if (data.Timeline is null)
            return "Missing required field: timeline";
        if (data.Timeline.Months is null || data.Timeline.Months.Count == 0)
            return "Missing required field: timeline.months (must have at least 1 element)";
        if (data.Timeline.NowPosition < 0.0 || data.Timeline.NowPosition > 1.0)
            return "timeline.nowPosition must be between 0.0 and 1.0";

        // Tracks validation
        if (data.Tracks is null)
            return "Missing required field: tracks";

        for (var t = 0; t < data.Tracks.Count; t++)
        {
            var track = data.Tracks[t];
            if (string.IsNullOrWhiteSpace(track.Id))
                return $"Missing required field: tracks[{t}].id";
            if (string.IsNullOrWhiteSpace(track.Label))
                return $"Missing required field: tracks[{t}].label";
            if (string.IsNullOrWhiteSpace(track.Color))
                return $"Missing required field: tracks[{t}].color";

            if (track.Milestones is not null)
            {
                for (var m = 0; m < track.Milestones.Count; m++)
                {
                    var milestone = track.Milestones[m];
                    if (string.IsNullOrWhiteSpace(milestone.Type))
                        return $"Missing required field: tracks[{t}].milestones[{m}].type";
                    if (!ValidMilestoneTypes.Contains(milestone.Type))
                        return $"Invalid milestone type: '{milestone.Type}' in tracks[{t}].milestones[{m}]. Must be one of: checkpoint, poc, production";
                    if (milestone.Position < 0.0 || milestone.Position > 1.0)
                        return $"tracks[{t}].milestones[{m}].position must be between 0.0 and 1.0";
                    if (string.IsNullOrWhiteSpace(milestone.Date))
                        return $"Missing required field: tracks[{t}].milestones[{m}].date";
                }
            }
        }

        // Heatmap validation
        if (data.Heatmap is null)
            return "Missing required field: heatmap";
        if (data.Heatmap.Categories is null)
            return "Missing required field: heatmap.categories";
        if (data.Heatmap.Months is null || data.Heatmap.Months.Count == 0)
            return "Missing required field: heatmap.months (must have at least 1 element)";

        for (var c = 0; c < data.Heatmap.Categories.Count; c++)
        {
            var category = data.Heatmap.Categories[c];
            if (string.IsNullOrWhiteSpace(category.Name))
                return $"Missing required field: heatmap.categories[{c}].name";
            if (string.IsNullOrWhiteSpace(category.CssClass))
                return $"Missing required field: heatmap.categories[{c}].cssClass";
            if (!ValidCssClasses.Contains(category.CssClass))
                return $"Invalid cssClass: '{category.CssClass}' in heatmap.categories[{c}]. Must be one of: ship, prog, carry, block";
            if (string.IsNullOrWhiteSpace(category.Emoji))
                return $"Missing required field: heatmap.categories[{c}].emoji";
            if (category.Items is null)
                return $"Missing required field: heatmap.categories[{c}].items";
        }

        return null;
    }
}