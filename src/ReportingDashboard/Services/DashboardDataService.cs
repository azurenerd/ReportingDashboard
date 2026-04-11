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
                _logger.LogError("ERROR: data.json not found at {Path}", filePath);
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            Data = JsonSerializer.Deserialize<DashboardData>(json, options);

            if (Data is null)
            {
                IsError = true;
                ErrorMessage = "data.json deserialized to null";
                _logger.LogError("ERROR: data.json deserialized to null");
                return;
            }

            IsError = false;
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(Data.Title))
                _logger.LogWarning("data.json: 'title' is empty");

            if (string.IsNullOrWhiteSpace(Data.Subtitle))
                _logger.LogWarning("data.json: 'subtitle' is empty");

            if (Data.Months.Count == 0)
                _logger.LogWarning("data.json: 'months' array is empty");

            if (Data.Timeline.Tracks.Count == 0)
                _logger.LogWarning("data.json: 'timeline.tracks' array is empty");

            if (Data.Heatmap.Shipped.Count == 0 &&
                Data.Heatmap.InProgress.Count == 0 &&
                Data.Heatmap.Carryover.Count == 0 &&
                Data.Heatmap.Blockers.Count == 0)
            {
                _logger.LogWarning("data.json: all heatmap categories are empty");
            }

            _logger.LogInformation(
                "Dashboard data loaded: {Title}, {TrackCount} tracks, {MonthCount} months",
                Data.Title, Data.Timeline.Tracks.Count, Data.Months.Count);
        }
        catch (JsonException ex)
        {
            IsError = true;
            ErrorMessage = $"Failed to parse data.json: {ex.Message} at line {ex.LineNumber}, position {ex.BytePositionInLine}";
            _logger.LogError("ERROR: Failed to parse data.json: {Message} at line {LineNumber}, position {BytePosition}",
                ex.Message, ex.LineNumber, ex.BytePositionInLine);
        }
        catch (Exception ex)
        {
            IsError = true;
            ErrorMessage = $"Unexpected error loading data.json: {ex.Message}";
            _logger.LogError(ex, "ERROR: Unexpected error loading data.json");
        }
    }
}