using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataService(ILogger<DashboardDataService> logger)
    {
        _logger = logger;
    }

    public DashboardData? Data { get; private set; }
    public bool IsError { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task LoadAsync(string filePath)
    {
        Data = null;
        IsError = false;
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            SetError("File path is null or empty");
            return;
        }

        if (!File.Exists(filePath))
        {
            SetError($"data.json not found at {filePath}");
            return;
        }

        string json;
        try
        {
            json = await File.ReadAllTextAsync(filePath);
        }
        catch (IOException ex)
        {
            SetError($"Failed to read data.json: {ex.Message}");
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            SetError($"Failed to read data.json (permission denied): {ex.Message}");
            return;
        }

        DashboardData? result;
        try
        {
            result = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            SetError($"Failed to parse data.json: {ex.Message}");
            return;
        }

        if (result is null)
        {
            SetError("data.json deserialized to null");
            return;
        }

        // Ensure non-null nested objects
        result.Timeline ??= new TimelineData();
        result.Heatmap ??= new HeatmapData();
        result.Months ??= new List<string>();

        var errors = Validate(result);
        if (errors.Count > 0)
        {
            var joined = string.Join("; ", errors);
            SetError($"data.json validation failed: {joined}");
            return;
        }

        // Warnings for optional fields
        if (string.IsNullOrWhiteSpace(result.BacklogLink))
            _logger.LogWarning("data.json: backlogLink is not set");

        if (string.IsNullOrWhiteSpace(result.Timeline.NowDate))
            _logger.LogWarning("data.json: timeline.nowDate is not set");

        if (result.Timeline.Tracks != null)
        {
            for (var i = 0; i < result.Timeline.Tracks.Count; i++)
            {
                var track = result.Timeline.Tracks[i];
                if (string.IsNullOrWhiteSpace(track.Name))
                    _logger.LogWarning("data.json: timeline.tracks[{Index}].name is empty", i);
                if (string.IsNullOrWhiteSpace(track.Label))
                    _logger.LogWarning("data.json: timeline.tracks[{Index}].label is empty", i);
            }
        }

        Data = result;
        _logger.LogInformation("Dashboard data loaded successfully: {Title}", result.Title);
    }

    private static List<string> Validate(DashboardData data)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(data.Title))
            errors.Add("title is required");

        if (string.IsNullOrWhiteSpace(data.Subtitle))
            errors.Add("subtitle is required");

        if (string.IsNullOrWhiteSpace(data.CurrentMonth))
            errors.Add("currentMonth is required");

        if (data.Months.Count == 0)
            errors.Add("months must be a non-empty array");

        if (data.Timeline is null)
        {
            errors.Add("timeline is required");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(data.Timeline.StartDate))
                errors.Add("timeline.startDate is required");

            if (string.IsNullOrWhiteSpace(data.Timeline.EndDate))
                errors.Add("timeline.endDate is required");

            if (data.Timeline.Tracks == null || data.Timeline.Tracks.Count == 0)
                errors.Add("timeline.tracks must be a non-empty array");
        }

        return errors;
    }

    private void SetError(string message)
    {
        IsError = true;
        ErrorMessage = message;
        _logger.LogError("DashboardDataService: {ErrorMessage}", message);
    }
}