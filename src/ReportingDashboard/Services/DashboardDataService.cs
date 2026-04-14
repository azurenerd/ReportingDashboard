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
                _logger.LogError("Dashboard data file not found: {Path}", filePath);
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };
            Data = JsonSerializer.Deserialize<DashboardData>(json, options);

            if (Data == null)
            {
                IsError = true;
                ErrorMessage = "data.json deserialized to null.";
                _logger.LogError("Failed to deserialize data.json");
                return;
            }

            if (string.IsNullOrEmpty(Data.Title))
            {
                IsError = true;
                ErrorMessage = "data.json validation: 'title' is required.";
                _logger.LogError("data.json validation failed: title is required");
                return;
            }

            if (Data.Months == null || Data.Months.Count == 0)
            {
                IsError = true;
                ErrorMessage = "data.json validation: 'months' array is required and must not be empty.";
                _logger.LogError("data.json validation failed: months is required");
                return;
            }

            IsError = false;
            ErrorMessage = null;
            _logger.LogInformation("Dashboard data loaded successfully from {Path}", filePath);
        }
        catch (JsonException ex)
        {
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message}";
            _logger.LogError(ex, "Failed to parse data.json");
        }
        catch (Exception ex)
        {
            IsError = true;
            ErrorMessage = $"Error loading data.json: {ex.Message}";
            _logger.LogError(ex, "Unexpected error loading data.json");
        }
    }
}