using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public partial class DashboardDataService : IDisposable
{
    private readonly string _contentRoot;
    private readonly ConcurrentDictionary<string, DashboardData> _cache = new();
    private readonly ILogger<DashboardDataService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private readonly object _debounceTimerLock = new();
    private bool _disposed;

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex ValidProjectNameRegex();

    [GeneratedRegex(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
    private static partial Regex ValidHexColorRegex();

    private static readonly string[] ValidMarkerTypes =
        { "checkpoint", "poc", "production", "smallCheckpoint" };
    private static readonly string[] ValidCategoryKeys =
        { "shipped", "inProgress", "carryover", "blockers" };

    /// <summary>
    /// Raised after a debounced file change successfully pre-validates and clears the cache.
    /// Subscribers (e.g., Dashboard.razor) should call InvokeAsync(StateHasChanged).
    /// </summary>
    public event Action? OnDataChanged;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _contentRoot = env.ContentRootPath;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    /// <summary>
    /// Validates the default data.json at startup and initializes the FileSystemWatcher.
    /// Call once from Program.cs after DI registration.
    /// </summary>
    public void Initialize()
    {
        var defaultPath = Path.Combine(_contentRoot, "data.json");
        if (File.Exists(defaultPath))
        {
            try
            {
                LoadAndValidate(string.Empty);
                _logger.LogInformation("Default data.json loaded and validated from {Path}", defaultPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load default data.json from {Path}", defaultPath);
                throw;
            }
        }
        else
        {
            _logger.LogWarning(
                "Default data.json not found at {Path}. Use ?project=<name> to load project-specific data.",
                defaultPath);
        }

        SetupFileWatcher();
    }

    private void SetupFileWatcher()
    {
        if (!Directory.Exists(_contentRoot))
            return;

        _watcher = new FileSystemWatcher(_contentRoot)
        {
            Filter = "data*.json",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
        };

        _watcher.Changed += OnFileSystemEvent;
        _watcher.Renamed += OnFileSystemRenamed;
        _watcher.EnableRaisingEvents = true;

        _logger.LogInformation(
            "FileSystemWatcher initialized, monitoring {Path} for data*.json changes",
            _contentRoot);
    }

    private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File system event: {ChangeType} on {FullPath}", e.ChangeType, e.FullPath);
        ScheduleReload();
    }

    private void OnFileSystemRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogDebug("File renamed: {OldFullPath} -> {FullPath}", e.OldFullPath, e.FullPath);
        ScheduleReload();
    }

    private void ScheduleReload()
    {
        lock (_debounceTimerLock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ => ExecuteReload(), null, 300, Timeout.Infinite);
        }
    }

    /// <summary>
    /// Attempts to re-read and validate data files, then clears the cache and raises OnDataChanged.
    /// If the read fails (malformed JSON, file locked), retains the last valid cached data.
    /// Marked internal for testability.
    /// </summary>
    internal void ExecuteReload()
    {
        try
        {
            PreValidateDataFiles();

            _cache.Clear();
            _logger.LogInformation("Dashboard data reloaded successfully after file change");
            OnDataChanged?.Invoke();
        }
        catch (IOException ex)
        {
            // File may be locked mid-write by the editor - retry once after 100ms
            _logger.LogDebug(ex, "IOException on first reload attempt, retrying in 100ms");
            try
            {
                Thread.Sleep(100);
                PreValidateDataFiles();

                _cache.Clear();
                _logger.LogInformation("Dashboard data reloaded successfully after retry");
                OnDataChanged?.Invoke();
            }
            catch (Exception retryEx)
            {
                _logger.LogWarning(
                    retryEx,
                    "Failed to reload data after file change (retry failed); retaining cached data");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to reload data after file change; retaining cached data");
        }
    }

    /// <summary>
    /// Pre-validates data files before clearing the cache. If any file fails to parse
    /// or validate, an exception propagates and the cache is left untouched.
    /// </summary>
    private void PreValidateDataFiles()
    {
        var defaultPath = Path.Combine(_contentRoot, "data.json");
        if (File.Exists(defaultPath))
        {
            ReadAndValidateFile(defaultPath);
        }

        // Also validate any project-specific data files that are cached
        foreach (var key in _cache.Keys)
        {
            if (string.IsNullOrEmpty(key))
                continue;

            try
            {
                var path = ResolveFilePath(key);
                if (File.Exists(path))
                {
                    ReadAndValidateFile(path);
                }
            }
            catch (DashboardDataException)
            {
                // Project file not found or invalid - skip (it will be re-validated on GetData)
            }
        }
    }

    /// <summary>
    /// Returns cached data for the given project, loading from disk on cache miss.
    /// </summary>
    public DashboardData GetData(string? projectName = null)
    {
        var key = projectName ?? string.Empty;
        return _cache.GetOrAdd(key, k => LoadAndValidate(k));
    }

    private DashboardData LoadAndValidate(string projectName)
    {
        var path = ResolveFilePath(projectName);
        return ReadAndValidateFile(path);
    }

    private DashboardData ReadAndValidateFile(string path)
    {
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions)
            ?? throw new DashboardDataException($"Failed to deserialize {path}");
        Validate(data, path, _logger);
        return data;
    }

    private string ResolveFilePath(string projectName)
    {
        if (string.IsNullOrEmpty(projectName))
        {
            var defaultPath = Path.Combine(_contentRoot, "data.json");
            if (!File.Exists(defaultPath))
                throw new DashboardDataException($"Dashboard data file not found: {defaultPath}");
            return defaultPath;
        }

        if (!ValidProjectNameRegex().IsMatch(projectName))
            throw new DashboardDataException(
                $"Invalid project name: '{projectName}'. " +
                "Only alphanumeric characters, hyphens, and underscores are allowed.");

        var primary = Path.Combine(_contentRoot, $"data.{projectName}.json");
        if (File.Exists(primary))
            return primary;

        var secondary = Path.Combine(_contentRoot, "projects", $"{projectName}.json");
        if (File.Exists(secondary))
            return secondary;

        throw new DashboardDataException(
            $"Project '{projectName}' not found. Searched: {primary}, {secondary}");
    }

    internal static void Validate(DashboardData data, string filePath, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            throw new DashboardDataException($"'title' is required in {filePath}");

        if (data.Months == null || data.Months.Count < 1 || data.Months.Count > 12)
            throw new DashboardDataException(
                $"'months' must contain 1-12 entries in {filePath}");

        if (data.CurrentMonthIndex < 0 || data.CurrentMonthIndex >= data.Months.Count)
            throw new DashboardDataException(
                $"'currentMonthIndex' ({data.CurrentMonthIndex}) is out of range " +
                $"for {data.Months.Count} months in {filePath}");

        if (data.TimelineStart >= data.TimelineEnd)
            throw new DashboardDataException(
                $"'timelineStart' must be before 'timelineEnd' in {filePath}");

        if (data.Milestones == null || data.Milestones.Count < 1 || data.Milestones.Count > 5)
            throw new DashboardDataException(
                $"'milestones' must contain 1-5 entries in {filePath}");

        foreach (var milestone in data.Milestones)
        {
            if (string.IsNullOrWhiteSpace(milestone.Color) ||
                !ValidHexColorRegex().IsMatch(milestone.Color))
            {
                throw new DashboardDataException(
                    $"Milestone '{milestone.Id}' has invalid color '{milestone.Color}' in {filePath}");
            }

            foreach (var marker in milestone.Markers)
            {
                if (!ValidMarkerTypes.Contains(marker.Type))
                    throw new DashboardDataException(
                        $"Marker type '{marker.Type}' is not recognized in {filePath}");
            }
        }

        if (data.Categories == null || data.Categories.Count != 4)
            throw new DashboardDataException(
                $"'categories' must contain exactly 4 entries in {filePath}");

        foreach (var category in data.Categories)
        {
            if (!ValidCategoryKeys.Contains(category.Key))
                throw new DashboardDataException(
                    $"Category key '{category.Key}' is not recognized in {filePath}");

            foreach (var itemKey in category.Items.Keys)
            {
                if (!data.Months.Contains(itemKey))
                    logger?.LogWarning(
                        "Category '{CategoryKey}' has items for month '{Month}' " +
                        "which is not in the months array",
                        category.Key, itemKey);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnFileSystemEvent;
                _watcher.Renamed -= OnFileSystemRenamed;
                _watcher.Dispose();
                _watcher = null;
            }

            lock (_debounceTimerLock)
            {
                _debounceTimer?.Dispose();
                _debounceTimer = null;
            }
        }

        _disposed = true;
    }
}