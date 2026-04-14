using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public partial class DashboardDataService : IDisposable
{
    private static readonly HashSet<string> ValidMarkerTypes = new(StringComparer.Ordinal)
    {
        "checkpoint", "poc", "production", "smallCheckpoint"
    };

    private static readonly HashSet<string> ValidCategoryKeys = new(StringComparer.Ordinal)
    {
        "shipped", "inProgress", "carryover", "blockers"
    };

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex ValidProjectNameRegex();

    private static readonly Regex ValidProjectName = ValidProjectNameRegex();
    private static readonly Regex ValidHexColor = new(@"^#[0-9A-Fa-f]{3,8}$", RegexOptions.Compiled);

    private readonly string _contentRoot;
    private readonly ConcurrentDictionary<string, DashboardData> _cache = new();
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private Timer? _debounceTimer;

    public event Action? OnDataChanged;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _contentRoot = env.ContentRootPath;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false
        };

        _watcher = new FileSystemWatcher(_contentRoot, "data*.json")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnFileChanged;
    }

    public void Initialize()
    {
        var defaultPath = Path.Combine(_contentRoot, "data.json");
        if (!File.Exists(defaultPath))
        {
            _logger.LogError("File not found: {Path}", defaultPath);
            throw new DashboardDataException($"Dashboard data file not found: {defaultPath}");
        }

        GetData(null);
        _logger.LogInformation("Dashboard data loaded successfully from {Path}", defaultPath);
    }

    public DashboardData GetData(string? projectName = null)
    {
        var key = projectName ?? string.Empty;
        return _cache.GetOrAdd(key, k => LoadAndValidate(k));
    }

    private DashboardData LoadAndValidate(string projectName)
    {
        var path = ResolveFilePath(projectName);
        var json = File.ReadAllText(path);

        DashboardData? data;
        try
        {
            data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parse error: {Path}", path);
            throw new DashboardDataException($"Failed to parse dashboard data: {ex.Message}");
        }

        if (data is null)
            throw new DashboardDataException($"Failed to deserialize {path}");

        Validate(data, path);
        return data;
    }

    internal string ResolveFilePath(string projectName)
    {
        if (!string.IsNullOrEmpty(projectName) && !ValidProjectName.IsMatch(projectName))
            throw new DashboardDataException(
                $"Invalid project name: '{projectName}'. Project names may only contain letters, numbers, hyphens, and underscores.");

        if (string.IsNullOrEmpty(projectName))
            return Path.Combine(_contentRoot, "data.json");

        var primary = Path.Combine(_contentRoot, $"data.{projectName}.json");
        if (File.Exists(primary)) return primary;

        var secondary = Path.Combine(_contentRoot, "projects", $"{projectName}.json");
        if (File.Exists(secondary)) return secondary;

        throw new DashboardDataException(
            $"Project '{projectName}' not found. Searched: {primary}, {secondary}");
    }

    private void Validate(DashboardData data, string filePath)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            throw new DashboardDataException($"'title' is required in {filePath}");

        if (data.Months is null || data.Months.Count < 1 || data.Months.Count > 12)
            throw new DashboardDataException($"'months' must contain 1-12 entries in {filePath}");

        if (data.CurrentMonthIndex < 0 || data.CurrentMonthIndex >= data.Months.Count)
            throw new DashboardDataException(
                $"'currentMonthIndex' ({data.CurrentMonthIndex}) is out of range for {data.Months.Count} months in {filePath}");

        if (data.TimelineStart >= data.TimelineEnd)
            throw new DashboardDataException($"'timelineStart' must be before 'timelineEnd' in {filePath}");

        if (data.Milestones is null || data.Milestones.Count < 1 || data.Milestones.Count > 5)
            throw new DashboardDataException($"'milestones' must contain 1-5 entries in {filePath}");

        foreach (var milestone in data.Milestones)
        {
            if (!ValidHexColor.IsMatch(milestone.Color))
                throw new DashboardDataException(
                    $"Milestone '{milestone.Id}' has invalid color '{milestone.Color}' in {filePath}");

            foreach (var marker in milestone.Markers)
            {
                if (!ValidMarkerTypes.Contains(marker.Type))
                    throw new DashboardDataException(
                        $"Marker type '{marker.Type}' is not recognized in {filePath}");
            }
        }

        if (data.Categories is null || data.Categories.Count != 4)
            throw new DashboardDataException($"'categories' must contain exactly 4 entries in {filePath}");

        foreach (var category in data.Categories)
        {
            if (!ValidCategoryKeys.Contains(category.Key))
                throw new DashboardDataException(
                    $"Category key '{category.Key}' is not recognized in {filePath}");
        }

        // Warn (non-fatal) if category item keys don't match months
        foreach (var category in data.Categories)
        {
            foreach (var itemKey in category.Items.Keys)
            {
                if (!data.Months.Contains(itemKey))
                    _logger.LogWarning(
                        "Category '{Key}' has items for month '{Month}' which is not in the months array",
                        category.Key, itemKey);
            }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            _logger.LogInformation("Data file changed: {File}, clearing cache", e.Name);
            _cache.Clear();
            OnDataChanged?.Invoke();
        }, null, 300, Timeout.Infinite);
    }

    public void Dispose()
    {
        _watcher.Dispose();
        _debounceTimer?.Dispose();
    }
}