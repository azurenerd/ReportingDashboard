using System.Text.Json;
using System.Text.Json.Serialization;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DataLoadException : Exception
{
    public DataLoadException(string message) : base(message) { }
}

public class DashboardDataService : IDashboardDataService
{
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardConfig? Data { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateOnly NowDate { get; private set; }
    public string CurrentColumn { get; private set; } = string.Empty;

    public DashboardDataService(ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        Load();
    }

    private void Load()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "data.json");
        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"data.json not found at: {path}", path);

            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var config = JsonSerializer.Deserialize<DashboardConfig>(json, options)
                ?? throw new DataLoadException("data.json deserialized to null.");

            Validate(config);

            Data = config;
            NowDate = config.Timeline.NowDate ?? DateOnly.FromDateTime(DateTime.Today);
            CurrentColumn = config.Heatmap.CurrentColumn
                ?? DateTime.Today.ToString("MMM");
            IsLoaded = true;

            _logger.LogInformation("data.json loaded. Project: {Title}", config.Project.Title);
        }
        catch (FileNotFoundException ex)
        {
            ErrorMessage = $"Could not load data.json: file not found. ({ex.Message})";
            _logger.LogError(ex, "{ErrorMessage}", ErrorMessage);
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"Could not parse data.json: invalid JSON near '{ex.Path}'. {ex.Message}";
            _logger.LogError(ex, "{ErrorMessage}", ErrorMessage);
        }
        catch (DataLoadException ex)
        {
            ErrorMessage = ex.Message;
            _logger.LogError(ex, "{ErrorMessage}", ErrorMessage);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error loading data.json: {ex.Message}";
            _logger.LogError(ex, "{ErrorMessage}", ErrorMessage);
        }
    }

    private static void Validate(DashboardConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Project.Title))
            throw new DataLoadException("Validation failed: project.title is required.");

        if (string.IsNullOrWhiteSpace(config.Project.Subtitle))
            throw new DataLoadException("Validation failed: project.subtitle is required.");

        if (config.Timeline.EndDate <= config.Timeline.StartDate)
            throw new DataLoadException("Validation failed: timeline.endDate must be after timeline.startDate.");

        if (config.Heatmap.Columns.Count == 0)
            throw new DataLoadException("Validation failed: heatmap.columns must contain at least one entry.");
    }
}