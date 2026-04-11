using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public virtual DashboardData? Data { get; private set; }
    public virtual bool IsError { get; private set; }
    public virtual string? ErrorMessage { get; private set; }

    public async Task LoadAsync(string? filePath = null)
    {
        var path = filePath ?? Path.Combine(_env.WebRootPath, "data.json");

        try
        {
            if (!File.Exists(path))
            {
                _logger.LogError("Dashboard data file not found at {Path}", path);
                IsError = true;
                ErrorMessage = $"Dashboard data file not found: {path}. Please create wwwroot/data.json.";
                return;
            }

            var json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                _logger.LogError("Failed to parse data.json: deserialized result was null");
                IsError = true;
                ErrorMessage = "Dashboard data file was empty or could not be parsed.";
                return;
            }

            if (string.IsNullOrWhiteSpace(data.Title))
            {
                _logger.LogWarning("data.json is missing required field 'title'");
            }

            if (data.Timeline.Tracks.Count == 0)
            {
                _logger.LogWarning("data.json contains no timeline tracks");
            }

            Data = data;
            IsError = false;
            ErrorMessage = null;
            _logger.LogInformation("Dashboard data loaded successfully from {Path}", path);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json: {Message}", ex.Message);
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading dashboard data: {Message}", ex.Message);
            IsError = true;
            ErrorMessage = $"Error loading dashboard data: {ex.Message}";
        }
    }
}