using System.Globalization;
using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataService(ILogger<DashboardDataService> logger)
    {
        _logger = logger;
    }

    public DashboardData? Data { get; private set; }
    public bool IsError { get; private set; }
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Loads and validates the dashboard data from the specified JSON file path.
    /// Must be called exactly once at application startup before app.Run().
    /// Not thread-safe; designed for single-call usage in Program.cs.
    /// </summary>
    public async Task LoadAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                SetError($"data.json not found at {filePath}");
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            DashboardData? data;
            try
            {
                data = JsonSerializer.Deserialize<DashboardData>(json, options);
            }
            catch (JsonException ex)
            {
                SetError($"Failed to parse data.json: {ex.Message}");
                return;
            }

            if (data is null)
            {
                SetError("Failed to parse data.json: deserialization returned null");
                return;
            }

            var validationError = Validate(data);
            if (validationError is not null)
            {
                SetError($"data.json validation: {validationError}");
                return;
            }

            Data = data;
            IsError = false;
            ErrorMessage = null;
            _logger.LogInformation(
                "Successfully loaded dashboard data: \"{Title}\" with {TrackCount} tracks and {MonthCount} months",
                data.Title, data.Timeline.Tracks.Count, data.Months.Count);
        }
        catch (Exception ex)
        {
            SetError($"Unexpected error loading data.json: {ex.Message}");
        }
    }

    private static string? Validate(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            return "title is required and must be non-empty";

        if (string.IsNullOrWhiteSpace(data.Subtitle))
            return "subtitle is required and must be non-empty";

        if (string.IsNullOrWhiteSpace(data.BacklogLink))
            return "backlogLink is required and must be non-empty";

        if (data.Months.Count == 0)
            return "months is required and must be non-empty";

        if (string.IsNullOrWhiteSpace(data.CurrentMonth))
            return "currentMonth is required and must be non-empty";

        if (!data.Months.Any(m => string.Equals(m, data.CurrentMonth, StringComparison.OrdinalIgnoreCase)))
            return "currentMonth must exist in months array";

        if (data.Timeline.Tracks.Count == 0)
            return "timeline.tracks is required and must be non-empty";

        if (string.IsNullOrWhiteSpace(data.Timeline.StartDate))
            return "timeline.startDate is required";

        if (!DateTime.TryParse(data.Timeline.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
            return $"timeline.startDate '{data.Timeline.StartDate}' is not a valid date";

        if (string.IsNullOrWhiteSpace(data.Timeline.EndDate))
            return "timeline.endDate is required";

        if (!DateTime.TryParse(data.Timeline.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            return $"timeline.endDate '{data.Timeline.EndDate}' is not a valid date";

        if (endDate <= startDate)
            return "timeline.endDate must be after timeline.startDate";

        if (string.IsNullOrWhiteSpace(data.Timeline.NowDate))
            return "timeline.nowDate is required";

        if (!DateTime.TryParse(data.Timeline.NowDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            return $"timeline.nowDate '{data.Timeline.NowDate}' is not a valid date";

        for (int i = 0; i < data.Timeline.Tracks.Count; i++)
        {
            var track = data.Timeline.Tracks[i];
            if (string.IsNullOrWhiteSpace(track.Name))
                return $"timeline.tracks[{i}].name is required";
            if (string.IsNullOrWhiteSpace(track.Label))
                return $"timeline.tracks[{i}].label is required";

            for (int j = 0; j < track.Milestones.Count; j++)
            {
                var milestone = track.Milestones[j];
                if (string.IsNullOrWhiteSpace(milestone.Date))
                    return $"timeline.tracks[{i}].milestones[{j}].date is required";
                if (!DateTime.TryParse(milestone.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return $"timeline.tracks[{i}].milestones[{j}].date '{milestone.Date}' is not a valid date";

                var validTypes = new[] { "checkpoint", "poc", "production" };
                if (!validTypes.Contains(milestone.Type, StringComparer.OrdinalIgnoreCase))
                    return $"timeline.tracks[{i}].milestones[{j}].type '{milestone.Type}' must be one of: checkpoint, poc, production";
            }
        }

        // Validate heatmap keys match lowercase month abbreviations from months array.
        // Convention: months array uses title-case ("Jan"), heatmap keys use lowercase ("jan").
        var expectedKeys = data.Months.Select(m => m.ToLowerInvariant()).ToHashSet();
        var heatmapCategories = new (string name, Dictionary<string, List<string>> dict)[]
        {
            ("shipped", data.Heatmap.Shipped),
            ("inProgress", data.Heatmap.InProgress),
            ("carryover", data.Heatmap.Carryover),
            ("blockers", data.Heatmap.Blockers)
        };

        foreach (var (name, dict) in heatmapCategories)
        {
            foreach (var key in dict.Keys)
            {
                if (!expectedKeys.Contains(key.ToLowerInvariant()))
                    return $"heatmap.{name} contains key '{key}' which does not match any month in the months array (expected lowercase: {string.Join(", ", expectedKeys)})";
            }
        }

        return null;
    }

    private void SetError(string message)
    {
        IsError = true;
        ErrorMessage = message;
        Data = null;
        _logger.LogError("Dashboard data error: {ErrorMessage}", message);
    }
}