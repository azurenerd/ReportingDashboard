using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly HashSet<string> ValidMilestoneTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "checkpoint", "poc", "production"
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
        if (!File.Exists(filePath))
        {
            IsError = true;
            ErrorMessage = $"data.json not found at {filePath}";
            _logger.LogWarning("Data file not found: {FilePath}", filePath);
            return;
        }

        DashboardData? data;
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            data = JsonSerializer.Deserialize<DashboardData>(json);
        }
        catch (JsonException ex)
        {
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message}";
            _logger.LogError(ex, "Failed to parse data.json at {FilePath}", filePath);
            return;
        }

        if (data is null)
        {
            IsError = true;
            ErrorMessage = "Failed to parse data.json: deserialization returned null";
            _logger.LogError("Deserialization of {FilePath} returned null", filePath);
            return;
        }

        var validationError = Validate(data);
        if (validationError is not null)
        {
            IsError = true;
            ErrorMessage = validationError;
            _logger.LogError("Validation failed for {FilePath}: {Error}", filePath, validationError);
            return;
        }

        Data = data;
        IsError = false;
        ErrorMessage = null;
    }

    private static string? Validate(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            return "data.json validation: 'title' is required";

        if (string.IsNullOrWhiteSpace(data.Subtitle))
            return "data.json validation: 'subtitle' is required";

        if (string.IsNullOrWhiteSpace(data.BacklogLink))
            return "data.json validation: 'backlogLink' is required";

        if (data.Months == null || data.Months.Count == 0)
            return "data.json validation: 'months' must be a non-empty array";

        if (!DateTime.TryParse(data.Timeline.StartDate, out _))
            return "data.json validation: 'timeline.startDate' must be a valid ISO date";

        if (!DateTime.TryParse(data.Timeline.EndDate, out _))
            return "data.json validation: 'timeline.endDate' must be a valid ISO date";

        if (!DateTime.TryParse(data.Timeline.NowDate, out _))
            return "data.json validation: 'timeline.nowDate' must be a valid ISO date";

        if (data.Timeline.Tracks == null || data.Timeline.Tracks.Count == 0)
            return "data.json validation: 'timeline.tracks' must be a non-empty array";

        foreach (var track in data.Timeline.Tracks)
        {
            foreach (var milestone in track.Milestones)
            {
                if (!ValidMilestoneTypes.Contains(milestone.Type))
                    return $"data.json validation: milestone type '{milestone.Type}' is invalid";
            }
        }

        return null;
    }
}