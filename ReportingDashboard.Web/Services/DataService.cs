using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DataService : IDataService, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _filePath;
    private readonly FileSystemWatcher? _watcher;
    private readonly Timer _debounceTimer;

    private volatile DashboardData? _data;
    private volatile string? _error;

    public event Action? OnDataChanged;

    public DataService(string contentRootPath)
    {
        _filePath = Path.Combine(contentRootPath, "data.json");
        _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);

        LoadData();

        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (directory is not null)
            {
                _watcher = new FileSystemWatcher(directory, "data.json")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
            }
        }
        catch
        {
            // FileSystemWatcher is a nice-to-have; don't fail startup if it can't be created
        }
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;

    private void LoadData()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _data = null;
                _error = $"data.json not found at {_filePath}. Create this file with your dashboard data.";
                return;
            }

            var json = File.ReadAllText(_filePath);
            _data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (_data is null)
            {
                _error = "Failed to parse data.json: deserialization returned null.";
            }
            else
            {
                _error = null;
            }
        }
        catch (JsonException ex)
        {
            _data = null;
            _error = $"Failed to parse data.json: {ex.Message}";
        }
        catch (IOException ex)
        {
            // Retry once after a brief delay for file-in-use scenarios
            try
            {
                Thread.Sleep(100);
                var json = File.ReadAllText(_filePath);
                _data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);
                _error = _data is null ? "Failed to parse data.json: deserialization returned null." : null;
            }
            catch (Exception retryEx)
            {
                _data = null;
                _error = $"Failed to read data.json: {retryEx.Message}";
            }
        }
        catch (Exception ex)
        {
            _data = null;
            _error = $"Failed to load data.json: {ex.Message}";
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce: reset timer to 300ms from now
        _debounceTimer.Change(300, Timeout.Infinite);
    }

    private void OnDebounceElapsed(object? state)
    {
        LoadData();
        OnDataChanged?.Invoke();
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _debounceTimer.Dispose();
    }
}