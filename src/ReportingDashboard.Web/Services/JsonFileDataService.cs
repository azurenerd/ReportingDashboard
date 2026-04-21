using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class JsonFileDataService : IDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JsonFileDataService> _logger;
    private DashboardData? _cachedData;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonFileDataService(
        IWebHostEnvironment env,
        IConfiguration configuration,
        ILogger<JsonFileDataService> logger)
    {
        _env = env;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<DashboardData> LoadDashboardDataAsync()
    {
        if (_cachedData is not null)
            return _cachedData;

        var relativePath = _configuration.GetValue<string>("DashboardOptions:DataFilePath")
                           ?? "wwwroot/data/data.sample.json";

        var filePath = Path.IsPathRooted(relativePath)
            ? relativePath
            : Path.Combine(_env.ContentRootPath, relativePath);

        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Data file not found at {Path}, returning default data", filePath);
                _cachedData = CreateDefaultData();
                return _cachedData;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                _logger.LogWarning("Data file deserialized to null, returning default data");
                _cachedData = CreateDefaultData();
                return _cachedData;
            }

            _logger.LogInformation("Dashboard data loaded from {Path}: {Title}", filePath, data.Title);
            _cachedData = data;
            return _cachedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data file from {Path}", filePath);
            _cachedData = CreateDefaultData();
            return _cachedData;
        }
    }

    private static DashboardData CreateDefaultData()
    {
        return new DashboardData
        {
            Title = "Dashboard",
            Subtitle = "No data loaded",
            BacklogLink = "",
            CurrentMonth = DateTime.Now.ToString("MMM"),
            Months = new List<string> { DateTime.Now.ToString("MMM") },
            Timeline = new TimelineData
            {
                StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
                EndDate = DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd"),
                NowDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Tracks = new List<TimelineTrack>()
            },
            Heatmap = new HeatmapData()
        };
    }
}