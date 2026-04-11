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
                _logger.LogError("data.json not found at {Path}", filePath);
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            Data = JsonSerializer.Deserialize<DashboardData>(json);

            if (Data is null)
            {
                IsError = true;
                ErrorMessage = "data.json deserialized to null.";
                _logger.LogError("data.json deserialized to null");
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
            _logger.LogError(ex, "Failed to parse data.json: {Details}", ex.Message);
        }
        catch (Exception ex)
        {
            IsError = true;
            ErrorMessage = $"Error loading data.json: {ex.Message}";
            _logger.LogError(ex, "Unexpected error loading data.json");
        }
    }
}