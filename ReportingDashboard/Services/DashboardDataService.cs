using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _dataFilePath;
    private DashboardData? _cachedData;
    private string? _error;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private volatile bool _cacheValid;
    private FileSystemWatcher? _watcher;

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
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += (_, _) => InvalidateCache();
                _watcher.Created += (_, _) => InvalidateCache();
            }
        }
        catch
        {
            // FileSystemWatcher is optional; ignore setup failures
        }
    }

    public async Task<DashboardData?> GetDashboardDataAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_cacheValid)
                return _cachedData;

            _error = null;
            _cachedData = null;

            if (!File.Exists(_dataFilePath))
            {
                _error = $"Unable to load dashboard data. File not found: {_dataFilePath}";
                _cacheValid = true;
                return null;
            }

            string json;
            try
            {
                json = await File.ReadAllTextAsync(_dataFilePath);
            }
            catch (IOException ex)
            {
                _error = $"Unable to load dashboard data. File read error: {ex.Message}";
                _cacheValid = true;
                return null;
            }

            DashboardData? data;
            try
            {
                data = JsonSerializer.Deserialize<DashboardData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _error = $"Unable to load dashboard data. JSON parse error: {ex.Message}";
                _cacheValid = true;
                return null;
            }

            if (data == null)
            {
                _error = "Unable to load dashboard data. JSON deserialized to null.";
                _cacheValid = true;
                return null;
            }

            // Apply null-coalescing defaults and log warnings for missing fields
            var title = data.Title;
            if (title == null)
            {
                Console.WriteLine("WARNING: 'title' field missing from data.json, defaulting to empty string.");
                title = "";
            }

            var subtitle = data.Subtitle;
            if (subtitle == null)
            {
                Console.WriteLine("WARNING: 'subtitle' field missing from data.json, defaulting to empty string.");
                subtitle = "";
            }

            var timelineMonths = data.TimelineMonths;
            if (timelineMonths == null)
            {
                Console.WriteLine("WARNING: 'timelineMonths' field missing from data.json, defaulting to empty array.");
                timelineMonths = Array.Empty<string>();
            }

            var currentMonth = data.CurrentMonth;
            if (currentMonth == null)
            {
                Console.WriteLine("WARNING: 'currentMonth' field missing from data.json, defaulting to empty string.");
                currentMonth = "";
            }

            var milestones = data.Milestones;
            if (milestones == null)
            {
                Console.WriteLine("WARNING: 'milestones' field missing from data.json, defaulting to empty array.");
                milestones = Array.Empty<MilestoneTrack>();
            }

            var heatmap = data.Heatmap;
            if (heatmap == null)
            {
                Console.WriteLine("WARNING: 'heatmap' field missing from data.json, defaulting to empty heatmap.");
                heatmap = new HeatmapData(Array.Empty<string>(), Array.Empty<HeatmapRow>());
            }

            _cachedData = data with
            {
                Title = title,
                Subtitle = subtitle,
                TimelineMonths = timelineMonths,
                CurrentMonth = currentMonth,
                Milestones = milestones,
                Heatmap = heatmap
            };

            _cacheValid = true;
            return _cachedData;
        }
        finally
        {
            _lock.Release();
        }
    }

    public string? GetError() => _error;

    public void InvalidateCache()
    {
        Volatile.Write(ref _cacheValid, false);
    }
}