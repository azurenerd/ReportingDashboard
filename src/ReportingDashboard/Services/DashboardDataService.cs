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
                SetError($"data.json not found at {filePath}");
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);

            DashboardData? data;
            try
            {
                data = JsonSerializer.Deserialize<DashboardData>(json);
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

            var validationError = ValidateData(data);
            if (validationError is not null)
            {
                SetError($"data.json validation: {validationError}");
                return;
            }

            Data = data;
            IsError = false;
            ErrorMessage = null;
            _logger.LogInformation("Dashboard data loaded successfully from {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            SetError($"Unexpected error loading data.json: {ex.Message}");
        }
    }

    private void SetError(string message)
    {
        IsError = true;
        ErrorMessage = message;
        Data = null;
        _logger.LogError("DashboardDataService error: {ErrorMessage}", message);
    }

    private static string? ValidateData(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            return "title is required";

        if (string.IsNullOrWhiteSpace(data.Subtitle))
            return "subtitle is required";

        if (string.IsNullOrWhiteSpace(data.BacklogLink))
            return "backlogLink is required";

        if (string.IsNullOrWhiteSpace(data.CurrentMonth))
            return "currentMonth is required";

        if (data.Months is null || data.Months.Count == 0)
            return "months is required and must not be empty";

        if (data.Timeline is null)
            return "timeline is required";

        if (data.Timeline.Tracks is null || data.Timeline.Tracks.Count == 0)
            return "timeline.tracks is required and must not be empty";

        if (string.IsNullOrWhiteSpace(data.Timeline.StartDate))
            return "timeline.startDate is required";

        if (string.IsNullOrWhiteSpace(data.Timeline.EndDate))
            return "timeline.endDate is required";

        if (string.IsNullOrWhiteSpace(data.Timeline.NowDate))
            return "timeline.nowDate is required";

        if (data.Heatmap is null)
            return "heatmap is required";

        return null;
    }
}