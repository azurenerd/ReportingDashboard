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
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("data.json not found at {Path}", filePath);
                IsError = true;
                ErrorMessage = $"data.json not found at {filePath}";
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                IsError = true;
                ErrorMessage = "data.json was empty or could not be parsed.";
                _logger.LogError("data.json deserialized to null");
                return;
            }

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(data.Title)) errors.Add("title is required");
            if (string.IsNullOrWhiteSpace(data.Subtitle)) errors.Add("subtitle is required");
            if (data.Months.Count == 0) errors.Add("months array must not be empty");
            if (data.Timeline.Tracks.Count == 0) errors.Add("timeline.tracks must not be empty");

            if (errors.Count > 0)
            {
                IsError = true;
                ErrorMessage = $"data.json validation: {string.Join("; ", errors)}";
                _logger.LogWarning("data.json validation failed: {Errors}", ErrorMessage);
                return;
            }

            Data = data;
            IsError = false;
            _logger.LogInformation("Dashboard data loaded successfully from {Path}", filePath);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json");
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading dashboard data");
            IsError = true;
            ErrorMessage = $"Error loading dashboard data: {ex.Message}";
        }
    }
}