using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly string[] ValidMilestoneTypes = { "checkpoint", "poc", "production" };

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

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            DashboardData? data;
            try
            {
                data = JsonSerializer.Deserialize<DashboardData>(json, options);
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

            var validationError = Validate(data);
            if (validationError is not null)
            {
                SetError(validationError);
                return;
            }

            Data = data;
            IsError = false;
            ErrorMessage = null;
            _logger.LogInformation("Successfully loaded dashboard data from {Path}", filePath);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load data.json: {ex.Message}");
        }
    }

    private string? Validate(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            return "data.json validation: 'title' is required";

        if (string.IsNullOrWhiteSpace(data.Subtitle))
            return "data.json validation: 'subtitle' is required";

        if (data.Months.Count == 0)
            return "data.json validation: 'months' must not be empty";

        if (data.Timeline.Tracks.Count == 0)
            return "data.json validation: 'timeline.tracks' must not be empty";

        foreach (var track in data.Timeline.Tracks)
        {
            foreach (var milestone in track.Milestones)
            {
                if (!ValidMilestoneTypes.Contains(milestone.Type))
                    return $"data.json validation: milestone type '{milestone.Type}' is not valid. Must be one of: {string.Join(", ", ValidMilestoneTypes)}";
            }
        }

        return null;
    }

    private void SetError(string message)
    {
        IsError = true;
        ErrorMessage = message;
        Data = null;
        _logger.LogError("DashboardDataService error: {Message}", message);
    }
}