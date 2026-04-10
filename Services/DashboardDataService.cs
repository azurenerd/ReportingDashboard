using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDashboardDataService
{
    private static readonly string[] ValidProjectStatuses = ["On Track", "At Risk", "Off Track"];
    private static readonly string[] ValidMilestoneStatuses = ["Completed", "In Progress", "Upcoming", "Delayed"];

    private readonly string _filePath;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DashboardData? Data { get; private set; }
    public string? LoadError { get; private set; }
    public bool IsLoaded => Data != null;
    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        _filePath = configuration.GetValue<string>("DashboardDataPath")
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "data.json");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        _logger.LogInformation("DashboardDataService initialized. Data file path: {FilePath}", _filePath);
    }

    public async Task LoadAsync()
    {
        _logger.LogInformation("Loading dashboard data from {FilePath}", _filePath);

        if (!File.Exists(_filePath))
        {
            LoadError = $"Dashboard data file not found at: {_filePath}";
            _logger.LogError("Dashboard data file not found at: {FilePath}", _filePath);
            OnDataChanged?.Invoke();
            return;
        }

        var json = await ReadFileWithRetryAsync();
        if (json is null)
        {
            LoadError = $"Unable to read dashboard data file at: {_filePath}. The file may be locked by another process.";
            _logger.LogError("All retry attempts exhausted reading {FilePath}", _filePath);
            OnDataChanged?.Invoke();
            return;
        }

        try
        {
            var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);

            if (data?.Project is null)
            {
                LoadError = "Invalid data: missing required 'project' section";
                _logger.LogError("Deserialized data is missing required 'project' section");
                OnDataChanged?.Invoke();
                return;
            }

            Data = data;
            LoadError = null;
            _logger.LogInformation("Dashboard data loaded successfully for project: {ProjectName}", Data.Project.Name);

            ValidateData();
        }
        catch (JsonException ex)
        {
            LoadError = $"Error loading data: {ex.Message}";
            _logger.LogError(ex, "Failed to deserialize dashboard data from {FilePath}", _filePath);
        }

        OnDataChanged?.Invoke();
    }

    private void ValidateData()
    {
        if (Data is null) return;

        if (string.IsNullOrWhiteSpace(Data.Project.Name))
        {
            _logger.LogWarning("Project name is empty or missing");
        }

        if (!ValidProjectStatuses.Contains(Data.Project.Status))
        {
            _logger.LogWarning("Unrecognized project status: '{Status}'. Will render with fallback styling.", Data.Project.Status);
        }

        if (Data.Milestones is null)
        {
            _logger.LogWarning("Milestones array is null in data.json. Defaulting to empty list.");
            Data = Data with { Milestones = new List<Milestone>() };
        }

        if (Data.Shipped is null)
        {
            _logger.LogWarning("Shipped array is null in data.json. Defaulting to empty list.");
            Data = Data with { Shipped = new List<WorkItem>() };
        }

        if (Data.InProgress is null)
        {
            _logger.LogWarning("InProgress array is null in data.json. Defaulting to empty list.");
            Data = Data with { InProgress = new List<WorkItemInProgress>() };
        }

        if (Data.CarriedOver is null)
        {
            _logger.LogWarning("CarriedOver array is null in data.json. Defaulting to empty list.");
            Data = Data with { CarriedOver = new List<CarriedOverItem>() };
        }

        if (Data.Metrics is null)
        {
            _logger.LogWarning("Metrics array is null in data.json. Defaulting to empty list.");
            Data = Data with { Metrics = new List<KeyMetric>() };
        }

        foreach (var milestone in Data.Milestones)
        {
            if (!DateOnly.TryParseExact(milestone.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                _logger.LogWarning("Milestone '{Title}' has unparseable date: '{Date}'. Expected format: yyyy-MM-dd.", milestone.Title, milestone.Date);
            }

            if (!ValidMilestoneStatuses.Contains(milestone.Status))
            {
                _logger.LogWarning("Milestone '{Title}' has unrecognized status: '{Status}'. Valid values: Completed, In Progress, Upcoming, Delayed.", milestone.Title, milestone.Status);
            }
        }

        foreach (var item in Data.InProgress)
        {
            if (item.PercentComplete < 0 || item.PercentComplete > 100)
            {
                _logger.LogWarning("In-progress item '{Title}' has PercentComplete value {Percent} outside valid range 0-100.", item.Title, item.PercentComplete);
            }
        }
    }

    private async Task<string?> ReadFileWithRetryAsync()
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (IOException ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "File read attempt {Attempt}/3 failed for {Path}. Retrying in 200ms.", attempt, _filePath);
                await Task.Delay(200);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "File read attempt {Attempt}/3 failed for {Path}. No more retries.", attempt, _filePath);
            }
        }

        return null;
    }

    public void Dispose()
    {
        // No resources to dispose yet - T3 adds FileSystemWatcher
    }
}