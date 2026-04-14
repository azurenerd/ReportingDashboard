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
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

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

    public event Action? OnDataChanged;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _contentRoot = env.ContentRootPath;
        _logger = logger;

        _watcher = new FileSystemWatcher(_contentRoot, "data*.json")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnFileChanged;
        _watcher.Created += OnFileChanged;
    }

    /// <summary>
    /// Validates that the default data.json is loadable at startup.
    /// </summary>
    public void Initialize()
    {
        var defaultPath = Path.Combine(_contentRoot, "data.json");
        if (!File.Exists(defaultPath))
        {
            _logger.LogWarning("Default data.json not found at {Path}. Dashboard will show an error until a data file is provided.", defaultPath);
            return;
        }

        try
        {
            var data = LoadAndValidate(string.Empty);
            _cache[string.Empty] = data;
            _logger.LogInformation("Successfully loaded default data.json: \"{Title}\"", data.Title);
        }
        catch (DashboardDataException ex)
        {
            _logger.LogError("Failed to load default data.json: {Message}", ex.Message);
        }
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
            throw new DashboardDataException($"Failed to read dashboard data file: {path} - {ex.Message}", ex);
        }

        DashboardData data;
        try
        {
            data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions)
                ?? throw new DashboardDataException($"Failed to deserialize {path}: file is empty or contains 'null'");
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

    private void Validate(DashboardData data, string filePath)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            throw new DashboardDataException($"'title' is required in {filePath}");

        if (data.Months == null || data.Months.Count < 1 || data.Months.Count > 12)
            throw new DashboardDataException($"'months' must contain 1-12 entries in {filePath}");

        if (data.CurrentMonthIndex < 0 || data.CurrentMonthIndex >= data.Months.Count)
            throw new DashboardDataException(
                $"'currentMonthIndex' ({data.CurrentMonthIndex}) is out of range for {data.Months.Count} months in {filePath}");

        if (data.TimelineStart >= data.TimelineEnd)
            throw new DashboardDataException($"'timelineStart' must be before 'timelineEnd' in {filePath}");

        if (data.Milestones == null || data.Milestones.Count < 1 || data.Milestones.Count > 5)
            throw new DashboardDataException($"'milestones' must contain 1-5 entries in {filePath}");

        foreach (var milestone in data.Milestones)
        {
            if (string.IsNullOrWhiteSpace(milestone.Id))
                throw new DashboardDataException($"Milestone is missing 'id' in {filePath}");

            if (string.IsNullOrWhiteSpace(milestone.Color) || !milestone.Color.StartsWith('#'))
                throw new DashboardDataException($"Milestone '{milestone.Id}' has invalid color '{milestone.Color}' in {filePath}");

            if (milestone.Markers != null)
            {
                foreach (var marker in milestone.Markers)
                {
                    if (!ValidMarkerTypes.Contains(marker.Type))
                        throw new DashboardDataException(
                            $"Marker type '{marker.Type}' is not recognized in milestone '{milestone.Id}'. Valid types: {string.Join(", ", ValidMarkerTypes)}");
                }
            }
        }

        if (data.Categories == null || data.Categories.Count != 4)
            throw new DashboardDataException($"'categories' must contain exactly 4 entries in {filePath}");

        foreach (var category in data.Categories)
        {
            if (!ValidCategoryKeys.Contains(category.Key))
                throw new DashboardDataException(
                    $"Category key '{category.Key}' is not recognized. Valid keys: {string.Join(", ", ValidCategoryKeys)}");

            if (category.Items != null)
            {
                foreach (var kvp in category.Items)
                {
                    if (!data.Months.Contains(kvp.Key))
                        _logger.LogWarning("Category '{Key}' has items for month '{Month}' which is not in the months array", category.Key, kvp.Key);

                    if (kvp.Value.Count > 8)
                        _logger.LogWarning("Warning: {Category}/{Month} has {Count} items; cells support up to 8 without overflow", category.Key, kvp.Key, kvp.Value.Count);
                }
            }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            _logger.LogInformation("Detected change in {FileName}, reloading data...", e.Name);
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
        if (_disposed) return;
        _disposed = true;
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _debounceTimer?.Dispose();
    }
}