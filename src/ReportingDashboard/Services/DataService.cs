using System.Globalization;
using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DataService : IDisposable
{
    private const int ExpectedSchemaVersion = 1;
    private readonly string _dataFilePath;
    private readonly object _lock = new();
    private readonly Timer _debounceTimer;
    private FileSystemWatcher? _watcher;

    private DashboardData? _currentData;
    private string? _currentError;

    public event Action? OnDataChanged;

    public DataService(IWebHostEnvironment env)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _dataFilePath = Path.Combine(webRoot, "data.json");
        _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);

        LoadData();
        StartWatcher(webRoot);
    }

    public DashboardData? GetData()
    {
        lock (_lock) return _currentData;
    }

    public string? GetError()
    {
        lock (_lock) return _currentError;
    }

    public DateOnly GetEffectiveDate()
    {
        DashboardData? data;
        lock (_lock) { data = _currentData; }

        if (data?.NowDateOverride is string s && !string.IsNullOrWhiteSpace(s))
        {
            if (DateOnly.TryParseExact(s, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var d))
                return d;
        }
        return DateOnly.FromDateTime(DateTime.Today);
    }

    public string GetCurrentMonthName()
    {
        DashboardData? data;
        lock (_lock) { data = _currentData; }

        if (data?.CurrentMonthOverride is string cmo && !string.IsNullOrWhiteSpace(cmo))
            return cmo;

        return GetEffectiveDate().ToString("MMM", CultureInfo.InvariantCulture);
    }

    private void StartWatcher(string directory)
    {
        if (!Directory.Exists(directory)) return;

        _watcher = new FileSystemWatcher(directory, "data.json")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName
        };
        _watcher.Changed += (_, _) => ScheduleReload();
        _watcher.Created += (_, _) => ScheduleReload();
        _watcher.Renamed += (_, _) => ScheduleReload();
        _watcher.EnableRaisingEvents = true;
    }

    private void ScheduleReload()
    {
        _debounceTimer.Change(500, Timeout.Infinite);
    }

    private void OnDebounceElapsed(object? state)
    {
        LoadData();
        RaiseOnDataChanged();
    }

    private void RaiseOnDataChanged()
    {
        var handler = OnDataChanged;
        if (handler == null) return;

        foreach (var subscriber in handler.GetInvocationList())
        {
            try
            {
                ((Action)subscriber).Invoke();
            }
            catch
            {
                // Subscriber exceptions must not kill the reload pipeline
            }
        }
    }

    private void LoadData()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                lock (_lock)
                {
                    if (_currentData == null)
                        _currentError = "No data.json found. Place a valid data.json file in the wwwroot/ directory and restart.";
                    else
                        _currentError = "data.json not found - showing last valid data.";
                }
                return;
            }

            var json = File.ReadAllText(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json);

            if (data == null)
            {
                lock (_lock) { _currentError = "data.json contains invalid JSON: deserialized to null."; }
                return;
            }

            if (data.SchemaVersion != ExpectedSchemaVersion)
            {
                lock (_lock)
                {
                    _currentError = $"data.json schemaVersion is {data.SchemaVersion}, expected {ExpectedSchemaVersion}. Update your data.json to match the current schema.";
                }
                return;
            }

            lock (_lock)
            {
                _currentData = data;
                _currentError = null;
            }
        }
        catch (JsonException ex)
        {
            lock (_lock) { _currentError = $"data.json contains invalid JSON: {ex.Message}"; }
        }
        catch (IOException ex)
        {
            lock (_lock) { _currentError = $"Could not read data.json: {ex.Message}"; }
        }
    }

    public void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
        _debounceTimer.Dispose();
    }
}