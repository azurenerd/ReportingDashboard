using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DashboardData? Data { get; private set; }
    public bool IsError { get; private set; }
    public string? ErrorMessage { get; private set; }

    public DashboardDataService(ILogger<DashboardDataService> logger)
    {
        _logger = logger;
    }

    public async Task LoadAsync(string filePath)
    {
        // Reset state on each load
        Data = null;
        IsError = false;
        ErrorMessage = null;

        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("data.json not found at {Path}", filePath);
                IsError = true;
                ErrorMessage = $"data.json not found at {filePath}";
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data == null)
            {
                _logger.LogError("data.json deserialized to null");
                IsError = true;
                ErrorMessage = "data.json deserialized to null";
                return;
            }

            // Ensure non-null nested objects
            data.Timeline ??= new TimelineData();
            data.Heatmap ??= new HeatmapData();
            data.Months ??= new List<string>();

            // Validate required fields per architecture Data Validation Rules
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(data.Title))
                errors.Add("title is required");

            if (string.IsNullOrWhiteSpace(data.Subtitle))
                errors.Add("subtitle is required");

            if (string.IsNullOrWhiteSpace(data.BacklogLink))
                errors.Add("backlogLink is required");

            if (string.IsNullOrWhiteSpace(data.CurrentMonth))
                errors.Add("currentMonth is required");

            if (data.Months.Count == 0)
                errors.Add("months array must not be empty");

            if (string.IsNullOrWhiteSpace(data.Timeline.StartDate))
                errors.Add("timeline.startDate is required");

            if (string.IsNullOrWhiteSpace(data.Timeline.EndDate))
                errors.Add("timeline.endDate is required");

            if (data.Timeline.Tracks == null || data.Timeline.Tracks.Count == 0)
                errors.Add("timeline.tracks must not be empty");

            if (errors.Count > 0)
            {
                var message = $"data.json validation failed: {string.Join("; ", errors)}";
                _logger.LogError("{Message}", message);
                IsError = true;
                ErrorMessage = message;
                return;
            }

            // Warnings for optional fields
            if (string.IsNullOrWhiteSpace(data.Timeline.NowDate))
                _logger.LogWarning("data.json: timeline.nowDate is not set");

            if (data.Timeline.Tracks != null)
            {
                for (var i = 0; i < data.Timeline.Tracks.Count; i++)
                {
                    var track = data.Timeline.Tracks[i];
                    if (string.IsNullOrWhiteSpace(track.Name))
                        _logger.LogWarning("data.json: timeline.tracks[{Index}].name is empty", i);
                    if (string.IsNullOrWhiteSpace(track.Label))
                        _logger.LogWarning("data.json: timeline.tracks[{Index}].label is empty", i);
                }
            }

            Data = data;
            _logger.LogInformation("Dashboard data loaded successfully: {Title}", data.Title);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json");
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading data.json");
            IsError = true;
            ErrorMessage = $"Unexpected error loading data.json: {ex.Message}";
        }
    }
}