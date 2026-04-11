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
                SetError("Failed to parse data.json: deserialization returned null");
                return;
            }

            var validationErrors = Validate(data);
            if (validationErrors.Count > 0)
            {
                SetError($"data.json validation: {string.Join("; ", validationErrors)}");
                return;
            }

            Data = data;
            IsError = false;
            ErrorMessage = null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json");
            SetError($"Failed to parse data.json: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading dashboard data");
            SetError($"Error loading dashboard data: {ex.Message}");
        }
    }

    private void SetError(string message)
    {
        IsError = true;
        ErrorMessage = message;
        Data = null;
        _logger.LogError("{ErrorMessage}", message);
    }

    private static List<string> Validate(DashboardData data)
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

        if (data.Months is null || data.Months.Count == 0)
            errors.Add("months is required");

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
            if (data.Timeline.Tracks is null || data.Timeline.Tracks.Count == 0)
                errors.Add("timeline.tracks is required");
        }

        return errors;
    }
}