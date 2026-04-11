using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public DashboardDataService(ILogger<DashboardDataService> logger)
    {
        _logger = logger;
    }

    public DashboardData? Data { get; private set; }
    public bool IsError { get; private set; }
    public string? ErrorMessage { get; private set; }

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
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                SetError("data.json deserialized to null.");
                return;
            }

            var validationError = Validate(data);
            if (validationError is not null)
            {
                SetError(validationError);
                return;
            }

            Data = data;
            IsError = false;
            ErrorMessage = null;
            _logger.LogInformation("Dashboard data loaded successfully from {Path}", filePath);
        }
        catch (JsonException ex)
        {
            SetError($"Invalid JSON in data.json: {ex.Message}");
        }
        catch (Exception ex)
        {
            SetError($"Error loading data.json: {ex.Message}");
        }
    }

    private void SetError(string message)
    {
        IsError = true;
        ErrorMessage = message;
        Data = null;
        _logger.LogError("DashboardDataService error: {Error}", message);
    }

    private static string? Validate(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            return "Missing required field: title";
        if (string.IsNullOrWhiteSpace(data.Subtitle))
            return "Missing required field: subtitle";
        if (string.IsNullOrWhiteSpace(data.BacklogLink))
            return "Missing required field: backlogLink";
        if (string.IsNullOrWhiteSpace(data.CurrentMonth))
            return "Missing required field: currentMonth";
        if (data.Months.Count == 0)
            return "Missing required field: months (must be non-empty)";

        // Timeline and Heatmap default to new() when missing from JSON,
        // so check for empty/default state rather than null.
        if (string.IsNullOrWhiteSpace(data.Timeline.StartDate) &&
            string.IsNullOrWhiteSpace(data.Timeline.EndDate) &&
            data.Timeline.Tracks.Count == 0)
            return "Missing required field: timeline";
        if (data.Timeline.Tracks.Count == 0)
            return "Missing required field: timeline.tracks (must be non-empty)";
        foreach (var track in data.Timeline.Tracks)
        {
            if (string.IsNullOrWhiteSpace(track.Name))
                return "Each timeline track must have a name";
            if (string.IsNullOrWhiteSpace(track.Label))
                return $"Timeline track '{track.Name}' must have a label";
        }
        if (data.Heatmap.Shipped.Count == 0 &&
            data.Heatmap.InProgress.Count == 0 &&
            data.Heatmap.Carryover.Count == 0 &&
            data.Heatmap.Blockers.Count == 0)
            return "Missing required field: heatmap";

        return null;
    }
}