using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public partial class DashboardDataService : IDisposable
{
    private readonly string _contentRoot;
    private readonly ConcurrentDictionary<string, DashboardData> _cache = new();
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger<DashboardDataService> _logger;
    private Timer? _debounceTimer;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly string[] ValidMarkerTypes = { "checkpoint", "poc", "production", "smallCheckpoint" };
    private static readonly string[] ValidCategoryKeys = { "shipped", "inProgress", "carryover", "blockers" };

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex ValidProjectNameRegex();

    [GeneratedRegex(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
    private static partial Regex HexColorRegex();

    public event Action? OnDataChanged;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _contentRoot = env.ContentRootPath;
        _logger = logger;

        _watcher = new FileSystemWatcher(_contentRoot)
        {
            Filter = "*.json",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = false
        };
        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnFileChanged;
    }

    /// <summary>
    /// Attempts to load and validate the default data.json at startup.
    /// If the file does not exist, logs a warning and continues without throwing.
    /// The FileSystemWatcher is always started so hot-reload works once data.json appears.
    /// </summary>
    public void Initialize()
    {
        var defaultPath = Path.Combine(_contentRoot, "data.json");
        if (File.Exists(defaultPath))
        {
            var data = LoadAndValidate(string.Empty);
            _cache[string.Empty] = data;
            _logger.LogInformation("Dashboard data loaded from {Path}", defaultPath);
        }
        else
        {
            _logger.LogWarning("Default data.json not found at {Path}. Dashboard will show an error until the file is created.", defaultPath);
        }

        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("FileSystemWatcher started on {Path}", _contentRoot);
    }

    public DashboardData GetData(string? projectName = null)
    {
        var key = projectName ?? string.Empty;
        return _cache.GetOrAdd(key, k => LoadAndValidate(k));
    }

    private DashboardData LoadAndValidate(string projectName)
    {
        var path = ResolveFilePath(projectName);

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (FileNotFoundException)
        {
            throw new DashboardDataException($"Dashboard data file not found: {path}");
        }
        catch (IOException ex)
        {
            throw new DashboardDataException($"Failed to read dashboard data file: {path}", ex);
        }

        DashboardData data;
        try
        {
            data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions)
                ?? throw new DashboardDataException($"Failed to deserialize {path}: result was null");
        }
        catch (JsonException ex)
        {
            throw new DashboardDataException($"Failed to parse dashboard data: {ex.Message}", ex);
        }

        Validate(data, path);
        return data;
    }

    private string ResolveFilePath(string projectName)
    {
        if (string.IsNullOrEmpty(projectName))
            return Path.Combine(_contentRoot, "data.json");

        if (!ValidProjectNameRegex().IsMatch(projectName))
            throw new DashboardDataException($"Invalid project name: '{projectName}'. Only alphanumeric characters, hyphens, and underscores are allowed.");

        var primary = Path.Combine(_contentRoot, $"data.{projectName}.json");
        if (File.Exists(primary)) return primary;

        var secondary = Path.Combine(_contentRoot, "projects", $"{projectName}.json");
        if (File.Exists(secondary)) return secondary;

        throw new DashboardDataException(
            $"Project '{projectName}' not found. Searched: {primary}, {secondary}");
    }

    internal static void Validate(DashboardData data, string filepath)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            throw new DashboardDataException($"'title' is required in {filepath}");

        if (data.Months == null || data.Months.Count < 1 || data.Months.Count > 12)
            throw new DashboardDataException($"'months' must contain 1-12 entries in {filepath}");

        if (data.CurrentMonthIndex < 0 || data.CurrentMonthIndex >= data.Months.Count)
            throw new DashboardDataException(
                $"'currentMonthIndex' ({data.CurrentMonthIndex}) is out of range for {data.Months.Count} months in {filepath}");

        if (data.TimelineStart >= data.TimelineEnd)
            throw new DashboardDataException($"'timelineStart' must be before 'timelineEnd' in {filepath}");

        if (data.Milestones == null || data.Milestones.Count < 1 || data.Milestones.Count > 5)
            throw new DashboardDataException($"'milestones' must contain 1-5 entries in {filepath}");

        foreach (var milestone in data.Milestones)
        {
            if (!string.IsNullOrEmpty(milestone.Color) && !HexColorRegex().IsMatch(milestone.Color))
                throw new DashboardDataException($"Milestone '{milestone.Id}' has invalid color '{milestone.Color}' in {filepath}");

            foreach (var marker in milestone.Markers)
            {
                if (!ValidMarkerTypes.Contains(marker.Type))
                    throw new DashboardDataException($"Marker type '{marker.Type}' is not recognized in milestone '{milestone.Id}' in {filepath}");
            }
        }

        if (data.Categories == null || data.Categories.Count != 4)
            throw new DashboardDataException($"'categories' must contain exactly 4 entries in {filepath}");

        foreach (var category in data.Categories)
        {
            if (!ValidCategoryKeys.Contains(category.Key))
                throw new DashboardDataException($"Category key '{category.Key}' is not recognized in {filepath}");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            _logger.LogInformation("Data file changed: {Name}. Clearing cache.", e.Name);
            _cache.Clear();
            try
            {
                OnDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying data change subscribers");
            }
        }, null, 300, Timeout.Infinite);
    }

    public void Dispose()
    {
        _watcher.Dispose();
        _debounceTimer?.Dispose();
    }
}