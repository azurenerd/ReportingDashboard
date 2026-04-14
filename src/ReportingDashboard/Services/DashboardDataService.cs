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
                var msg = $"data.json not found at {filePath}";
                _logger.LogError("{Message}", msg);
                IsError = true;
                ErrorMessage = msg;
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json);

            if (data is null)
            {
                var msg = "Failed to parse data.json: deserialization returned null";
                _logger.LogError("{Message}", msg);
                IsError = true;
                ErrorMessage = msg;
                return;
            }

            var validationErrors = ValidateData(data);
            if (validationErrors.Count > 0)
            {
                var msg = $"data.json validation: {string.Join("; ", validationErrors)}";
                _logger.LogError("{Message}", msg);
                IsError = true;
                ErrorMessage = msg;
                return;
            }

            Data = data;
            IsError = false;
            _logger.LogInformation("Dashboard data loaded successfully from {Path}", filePath);
        }
        catch (JsonException ex)
        {
            var msg = $"Failed to parse data.json: {ex.Message}";
            _logger.LogError(ex, "{Message}", msg);
            IsError = true;
            ErrorMessage = msg;
        }
        catch (Exception ex)
        {
            var msg = $"Error loading data.json: {ex.Message}";
            _logger.LogError(ex, "{Message}", msg);
            IsError = true;
            ErrorMessage = msg;
        }
    }

    private static List<string> ValidateData(DashboardData data)
    {
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
        if (data.Timeline.Tracks.Count == 0)
            errors.Add("timeline.tracks must not be empty");
        if (string.IsNullOrWhiteSpace(data.Timeline.StartDate))
            errors.Add("timeline.startDate is required");
        if (string.IsNullOrWhiteSpace(data.Timeline.EndDate))
            errors.Add("timeline.endDate is required");
        if (string.IsNullOrWhiteSpace(data.Timeline.NowDate))
            errors.Add("timeline.nowDate is required");

        return errors;
    }
}