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
        // Reset state before each load
        Data = null;
        IsError = false;
        ErrorMessage = null;

        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("Data file not found at {Path}", filePath);
                IsError = true;
                ErrorMessage = $"data.json not found at {filePath}";
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                _logger.LogError("data.json deserialized to null");
                IsError = true;
                ErrorMessage = "Dashboard data could not be loaded. Check data.json for errors.";
                return;
            }

            // Collect all validation errors
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(data.Title))
                errors.Add("title is required");

            if (string.IsNullOrWhiteSpace(data.Subtitle))
                errors.Add("subtitle is required");

            if (string.IsNullOrWhiteSpace(data.BacklogLink))
                errors.Add("backlogLink is required");

            if (data.Months.Count == 0)
                errors.Add("months is required and must be non-empty");

            if (string.IsNullOrWhiteSpace(data.Timeline.StartDate))
                errors.Add("timeline.startDate is required");

            if (string.IsNullOrWhiteSpace(data.Timeline.EndDate))
                errors.Add("timeline.endDate is required");

            if (string.IsNullOrWhiteSpace(data.Timeline.NowDate))
                errors.Add("timeline.nowDate is required");

            if (data.Timeline.Tracks.Count == 0)
                errors.Add("timeline.tracks is required and must be non-empty");

            if (errors.Count > 0)
            {
                var joined = string.Join("; ", errors);
                var message = $"data.json validation: {joined}";
                _logger.LogError("Validation failed: {Errors}", message);
                IsError = true;
                ErrorMessage = message;
                return;
            }

            Data = data;
            IsError = false;
            ErrorMessage = null;
            _logger.LogInformation("Dashboard data loaded successfully: {Title}", data.Title);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json: {Message}", ex.Message);
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message}";
            Data = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading dashboard data");
            IsError = true;
            ErrorMessage = $"Error loading dashboard data: {ex.Message}";
            Data = null;
        }
    }
}