using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardData? Data { get; private set; }
    public bool IsError { get; private set; }
    public string? ErrorMessage { get; private set; }

    public DashboardDataService(ILogger<DashboardDataService> logger)
    {
        _logger = logger;
    }

    public async Task LoadAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                IsError = true;
                ErrorMessage = $"data.json not found at {filePath}";
                _logger.LogError("Data file not found: {Path}", filePath);
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            if (data is null)
            {
                IsError = true;
                ErrorMessage = "data.json deserialized to null.";
                _logger.LogError("Deserialized data.json was null");
                return;
            }

            var validationError = Validate(data);
            if (validationError is not null)
            {
                IsError = true;
                ErrorMessage = $"data.json validation: {validationError}";
                _logger.LogError("Validation failed: {Error}", validationError);
                return;
            }

            Data = data;
            IsError = false;
            _logger.LogInformation("Dashboard data loaded successfully from {Path}", filePath);
        }
        catch (JsonException ex)
        {
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message}";
            _logger.LogError(ex, "JSON parse error");
        }
        catch (Exception ex)
        {
            IsError = true;
            ErrorMessage = $"Error loading data.json: {ex.Message}";
            _logger.LogError(ex, "Unexpected error loading dashboard data");
        }
    }

    private string? Validate(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            return "title is required";
        if (string.IsNullOrWhiteSpace(data.Subtitle))
            return "subtitle is required";
        if (data.Months is null || data.Months.Count == 0)
            return "months array is required and must not be empty";
        if (string.IsNullOrWhiteSpace(data.CurrentMonth))
            return "currentMonth is required";
        if (data.Timeline is null)
            return "timeline is required";
        if (string.IsNullOrWhiteSpace(data.Timeline.StartDate))
            return "timeline.startDate is required";
        if (string.IsNullOrWhiteSpace(data.Timeline.EndDate))
            return "timeline.endDate is required";
        if (data.Timeline.Tracks is null || data.Timeline.Tracks.Count == 0)
            return "timeline.tracks must not be empty";
        if (data.Heatmap is null)
            return "heatmap is required";
        if (string.IsNullOrWhiteSpace(data.BacklogLink))
            _logger.LogWarning("backlogLink is empty in data.json");

        return null;
    }
}