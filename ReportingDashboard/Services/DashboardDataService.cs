using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataServiceOptions
{
    public string FilePath { get; set; } = "data.json";
}

public class DashboardDataService : IDisposable
{
    private readonly string _filePath;
    private readonly FileSystemWatcher _watcher;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();
    private DashboardData? _data;
    private string? _errorMessage;

    public DashboardData? Data
    {
        get { lock (_lock) return _data; }
    }

    public string? ErrorMessage
    {
        get { lock (_lock) return _errorMessage; }
    }

    public event Action? OnDataChanged;

    public DashboardDataService(DashboardDataServiceOptions options)
    {
        _filePath = options.FilePath;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath)) ?? Directory.GetCurrentDirectory();
        var fileName = Path.GetFileName(_filePath);

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnFileChanged;
    }

    public void Initialize()
    {
        LoadData();
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Thread.Sleep(300);
        LoadData();
        OnDataChanged?.Invoke();
    }

    private void LoadData()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _data = null;
                    _errorMessage = $"Data file not found: {_filePath}";
                    return;
                }

                var json = File.ReadAllText(_filePath);
                _data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
                _errorMessage = _data == null ? "Data file is empty." : null;
            }
            catch (JsonException ex)
            {
                _data = null;
                _errorMessage = $"Invalid JSON in data file: {ex.Message}";
            }
            catch (IOException ex)
            {
                _errorMessage = $"Could not read data file: {ex.Message}";
            }
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}