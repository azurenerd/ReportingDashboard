using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _dataFilePath;
    private DashboardData? _cachedData;
    private string? _error;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly FileSystemWatcher? _watcher;
    private volatile bool _cacheInvalidated = true;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.WebRootPath, "data", "data.json");

        try
        {
            var directory = Path.GetDirectoryName(_dataFilePath);
            if (directory != null && Directory.Exists(directory))
            {
                _watcher = new FileSystemWatcher(directory, "data.json")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += (_, _) => InvalidateCache();
                _watcher.Created += (_, _) => InvalidateCache();
                _watcher.Deleted += (_, _) => InvalidateCache();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: FileSystemWatcher could not be initialized: {ex.Message}");
        }
    }

    public async Task<DashboardData?> GetDashboardDataAsync()
    {
        if (!_cacheInvalidated && _cachedData != null)
            return _cachedData;

        await _lock.WaitAsync();
        try
        {
            if (!_cacheInvalidated && _cachedData != null)
                return _cachedData;

            _error = null;
            _cachedData = null;

            if (!File.Exists(_dataFilePath))
            {
                _error = $"Unable to load dashboard data. File not found: {_dataFilePath}";
                return null;
            }

            var json = await File.ReadAllTextAsync(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data == null)
            {
                _error = "Unable to load dashboard data. JSON parse error: deserialization returned null.";
                return null;
            }

            data = ApplyDefaults(data);
            _cachedData = data;
            _cacheInvalidated = false;
            return data;
        }
        catch (FileNotFoundException)
        {
            _error = $"Unable to load dashboard data. File not found: {_dataFilePath}";
            return null;
        }
        catch (JsonException ex)
        {
            _error = $"Unable to load dashboard data. JSON parse error: {ex.Message}";
            return null;
        }
        catch (IOException ex)
        {
            _error = $"Unable to load dashboard data. File read error: {ex.Message}";
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public string? GetError() => _error;

    public void InvalidateCache()
    {
        _cacheInvalidated = true;
    }

    private static DashboardData ApplyDefaults(DashboardData data)
    {
        var title = data.Title;
        var subtitle = data.Subtitle;
        var timelineMonths = data.TimelineMonths;
        var currentMonth = data.CurrentMonth;
        var milestones = data.Milestones;
        var heatmap = data.Heatmap;

        if (title == null)
        {
            Console.WriteLine("Warning: 'title' field missing from data.json, using empty string.");
            title = "";
        }
        if (subtitle == null)
        {
            Console.WriteLine("Warning: 'subtitle' field missing from data.json, using empty string.");
            subtitle = "";
        }
        if (timelineMonths == null)
        {
            Console.WriteLine("Warning: 'timelineMonths' field missing from data.json, using empty array.");
            timelineMonths = Array.Empty<string>();
        }
        if (currentMonth == null)
        {
            Console.WriteLine("Warning: 'currentMonth' field missing from data.json, using empty string.");
            currentMonth = "";
        }
        if (milestones == null)
        {
            Console.WriteLine("Warning: 'milestones' field missing from data.json, using empty array.");
            milestones = Array.Empty<MilestoneTrack>();
        }
        if (heatmap == null)
        {
            Console.WriteLine("Warning: 'heatmap' field missing from data.json, using empty heatmap.");
            heatmap = new HeatmapData(Array.Empty<string>(), Array.Empty<HeatmapRow>());
        }

        return data with
        {
            Title = title,
            Subtitle = subtitle,
            TimelineMonths = timelineMonths,
            CurrentMonth = currentMonth,
            Milestones = milestones,
            Heatmap = heatmap
        };
    }
}