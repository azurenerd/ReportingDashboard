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

    public async Task LoadAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                IsError = true;
                ErrorMessage = $"data.json not found at {filePath}";
                _logger.LogError(ErrorMessage);
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                AllowTrailingCommas = false
            };

            var data = JsonSerializer.Deserialize<DashboardData>(json, options);

            if (data is null)
            {
                IsError = true;
                ErrorMessage = "Failed to parse data.json: deserialization returned null";
                _logger.LogError(ErrorMessage);
                return;
            }

            // Validate required fields
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(data.Title))
                validationErrors.Add("title is required");

            if (string.IsNullOrWhiteSpace(data.Subtitle))
                validationErrors.Add("subtitle is required");

            if (string.IsNullOrWhiteSpace(data.BacklogLink))
                validationErrors.Add("backlogLink is required");

            if (string.IsNullOrWhiteSpace(data.CurrentMonth))
                validationErrors.Add("currentMonth is required");

            if (data.Months.Count == 0)
                validationErrors.Add("months array must not be empty");

            if (string.IsNullOrWhiteSpace(data.Timeline.StartDate))
                validationErrors.Add("timeline.startDate is required");

            if (string.IsNullOrWhiteSpace(data.Timeline.EndDate))
                validationErrors.Add("timeline.endDate is required");

            if (string.IsNullOrWhiteSpace(data.Timeline.NowDate))
                validationErrors.Add("timeline.nowDate is required");

            if (data.Timeline.Tracks.Count == 0)
                validationErrors.Add("timeline.tracks array must not be empty");

            if (validationErrors.Count > 0)
            {
                IsError = true;
                ErrorMessage = $"data.json validation: {string.Join("; ", validationErrors)}";
                _logger.LogError(ErrorMessage);
                return;
            }

            Data = data;
            IsError = false;
            _logger.LogInformation("data.json loaded successfully: \"{Title}\" with {TrackCount} tracks and {MonthCount} months",
                data.Title, data.Timeline.Tracks.Count, data.Months.Count);
        }
        catch (JsonException ex)
        {
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message}";
            _logger.LogError(ErrorMessage);
        }
        catch (Exception ex)
        {
            IsError = true;
            ErrorMessage = $"Unexpected error loading data.json: {ex.Message}";
            _logger.LogError(ex, ErrorMessage);
        }
    }
}