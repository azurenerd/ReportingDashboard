using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDisposable
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private DashboardData? _data;
    private string? _error;
    private FileSystemWatcher? _watcher;
    private Timer? _pollTimer;
    private DateTime _lastWriteTime;

    public event Action? OnDataChanged;

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;

    public DashboardDataService(IConfiguration config, IWebHostEnvironment env)
    {
        var relativePath = config["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";
        _filePath = Path.IsPathRooted(relativePath)
            ? relativePath
            : Path.Combine(env.ContentRootPath, relativePath);

        LoadData();
        StartWatching();
    }

    private void LoadData()
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _data = null;
                    _error = $"Dashboard data file not found. Expected location: {_filePath}";
                    return;
                }

                var json = File.ReadAllText(_filePath);
                _data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
                if (_data?.Project == null || _data?.Timeline == null || _data?.Heatmap == null)
                {
                    _data = null;
                    _error = "Error reading dashboard data: JSON is missing required sections (project, timeline, or heatmap).";
                    return;
                }
                _error = null;
                return;
            }
            catch (IOException) when (attempt < 2)
            {
                Thread.Sleep(200);
            }
            catch (JsonException ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                return;
            }
            catch (Exception ex)
            {
                _data = null;
                _error = $"Error reading dashboard data: {ex.Message}";
                return;
            }
        }
    }

    private void StartWatching()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            var file = Path.GetFileName(_filePath);
            if (dir != null && Directory.Exists(dir))
            {
                _watcher = new FileSystemWatcher(dir, file)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                _watcher.Renamed += OnFileChanged;
            }
        }
        catch
        {
            // FileSystemWatcher may not be supported; polling fallback handles it
        }

        if (File.Exists(_filePath))
        {
            _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);
        }

        _pollTimer = new Timer(PollForChanges, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Thread.Sleep(300); // debounce
        ReloadAndNotify();
    }

    private void ReloadAndNotify()
    {
        LoadData();
        try { if (File.Exists(_filePath)) _lastWriteTime = File.GetLastWriteTimeUtc(_filePath); } catch { }
        OnDataChanged?.Invoke();
    }

    private void PollForChanges(object? state)
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var lwt = File.GetLastWriteTimeUtc(_filePath);
                if (lwt != _lastWriteTime)
                {
                    _lastWriteTime = lwt;
                    LoadData();
                    OnDataChanged?.Invoke();
                }
            }
            else if (_data != null)
            {
                LoadData();
                OnDataChanged?.Invoke();
            }
        }
        catch
        {
            // Ignore polling errors; next poll will retry
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollTimer?.Dispose();
    }
}
