using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDisposable
{
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private FileSystemWatcher? _fileWatcher;
    private Timer? _debounceTimer;
    private readonly int _debounceMs;

    public event Action? OnDataChanged;

    public DashboardDataService(IWebHostEnvironment env, IConfiguration config)
    {
        var configuredPath = config["Dashboard:DataFilePath"];
        if (!string.IsNullOrEmpty(configuredPath) && Path.IsPathRooted(configuredPath))
        {
            _dataFilePath = configuredPath;
        }
        else if (!string.IsNullOrEmpty(configuredPath))
        {
            _dataFilePath = Path.Combine(env.ContentRootPath, configuredPath);
        }
        else
        {
            _dataFilePath = Path.Combine(env.WebRootPath, "data", "data.json");
        }

        _debounceMs = config.GetValue("Dashboard:AutoRefreshDebounceMs", 300);
        var enableAutoRefresh = config.GetValue("Dashboard:EnableAutoRefresh", true);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (enableAutoRefresh)
        {
            InitializeFileWatcher();
        }
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
            return data ?? new DashboardData();
        }
        catch (FileNotFoundException)
        {
            return new DashboardData
            {
                ErrorMessage = $"data.json not found at {_dataFilePath}. Please ensure the file exists."
            };
        }
        catch (DirectoryNotFoundException)
        {
            return new DashboardData
            {
                ErrorMessage = $"data.json not found at {_dataFilePath}. The directory does not exist."
            };
        }
        catch (JsonException ex)
        {
            return new DashboardData
            {
                ErrorMessage = $"Invalid JSON in data.json: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new DashboardData
            {
                ErrorMessage = $"Unable to read data.json: {ex.Message}"
            };
        }
    }

    private void InitializeFileWatcher()
    {
        var directory = Path.GetDirectoryName(_dataFilePath);
        var fileName = Path.GetFileName(_dataFilePath);

        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            return;

        _fileWatcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };

        _fileWatcher.Changed += OnFileChanged;
        _fileWatcher.Created += OnFileChanged;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ => OnDataChanged?.Invoke(), null, _debounceMs, Timeout.Infinite);
    }

    public void Dispose()
    {
        _fileWatcher?.Dispose();
        _debounceTimer?.Dispose();
    }
}