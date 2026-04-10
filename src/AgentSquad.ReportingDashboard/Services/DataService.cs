using Microsoft.Extensions.Configuration;
using AgentSquad.ReportingDashboard.Models;
using System.Text.Json;

namespace AgentSquad.ReportingDashboard.Services;

public class DataService : IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;
    private string _dataFilePath = string.Empty;
    private FileSystemWatcher? _fileWatcher;
    private CancellationTokenSource? _debounceCts;

    public DashboardData CurrentData { get; private set; } = new();
    public event Func<Task>? OnDataChanged;

    public DataService(IConfiguration configuration)
    {
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _dataFilePath = _configuration["Dashboard:DataFilePath"] ?? "wwwroot/data.json";
    }

    public async Task InitializeAsync()
    {
        try
        {
            var json = File.ReadAllText(_dataFilePath);
            var loadedData = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);

            CurrentData = loadedData ?? GetDefaultData();
            System.Diagnostics.Debug.WriteLine($"DataService: Loaded data from {_dataFilePath}");
        }
        catch (FileNotFoundException)
        {
            System.Diagnostics.Debug.WriteLine($"DataService: {_dataFilePath} not found; using empty defaults");
            CurrentData = GetDefaultData();
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"DataService: JSON parse error: {ex.Message}");
            CurrentData = GetDefaultData();
        }
        catch (IOException ex)
        {
            System.Diagnostics.Debug.WriteLine($"DataService: File access error: {ex.Message}");
            CurrentData = GetDefaultData();
        }

        SetupFileWatcher();
        await Task.CompletedTask;
    }

    private void SetupFileWatcher()
    {
        try
        {
            var dirPath = Path.GetDirectoryName(_dataFilePath);
            if (string.IsNullOrEmpty(dirPath))
                dirPath = ".";

            _fileWatcher = new FileSystemWatcher(dirPath)
            {
                Filter = "data.json",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _fileWatcher.Created += OnFileChanged;
            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.Renamed += OnFileChanged;

            _fileWatcher.EnableRaisingEvents = true;
            System.Diagnostics.Debug.WriteLine("DataService: FileSystemWatcher initialized for data.json");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DataService: Failed to setup FileSystemWatcher: {ex.Message}");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("DataService: FileSystemWatcher detected data.json change");
        _ = DebouncedReload();
    }

    private async Task DebouncedReload()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();

        var cts = new CancellationTokenSource();
        _debounceCts = cts;

        try
        {
            await Task.Delay(500, cts.Token);
            await ReloadDataAsync();
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("DataService: Debounce reload cancelled by newer event");
        }
    }

    public async Task<DashboardData> ReloadDataAsync()
    {
        string? json = null;
        bool readSuccess = false;

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                using var fs = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(fs);
                json = await reader.ReadToEndAsync();
                readSuccess = true;
                System.Diagnostics.Debug.WriteLine($"DataService: Successfully read data.json on attempt {attempt + 1}");
                break;
            }
            catch (IOException) when (attempt < 2)
            {
                int delayMs = 100 * (attempt + 1);
                System.Diagnostics.Debug.WriteLine($"DataService: File read attempt {attempt + 1} failed, retrying in {delayMs}ms");
                await Task.Delay(delayMs);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: File read failed after 3 attempts: {ex.Message}");
            }
        }

        if (readSuccess && !string.IsNullOrEmpty(json))
        {
            try
            {
                var loadedData = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
                if (loadedData != null)
                {
                    CurrentData = loadedData;
                    System.Diagnostics.Debug.WriteLine("DataService: Successfully deserialized JSON data");
                }
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: JSON parse error on reload: {ex.Message}");
            }
        }
        else
        {
            try
            {
                var json2 = File.ReadAllText(_dataFilePath);
                var loadedData = JsonSerializer.Deserialize<DashboardData>(json2, _jsonOptions);
                if (loadedData != null)
                {
                    CurrentData = loadedData;
                }
            }
            catch (FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("DataService: File not found during reload");
                CurrentData = GetDefaultData();
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: JSON parse error: {ex.Message}");
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: IO error during reload: {ex.Message}");
            }
        }

        await (OnDataChanged?.Invoke() ?? Task.CompletedTask);
        return CurrentData;
    }

    private static DashboardData GetDefaultData()
    {
        return new DashboardData
        {
            ProjectName = "Dashboard",
            ProjectStatus = "Unknown",
            Milestones = new(),
            ShippedItems = new(),
            InProgressItems = new(),
            CarryoverItems = new(),
            LastUpdated = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        if (_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Dispose();
            System.Diagnostics.Debug.WriteLine("DataService: FileSystemWatcher disposed");
        }

        _debounceCts?.Dispose();
        System.Diagnostics.Debug.WriteLine("DataService: CancellationTokenSource disposed");
    }

    public async ValueTask DisposeAsync()
    {
        Dispose();
        await ValueTask.CompletedTask;
    }
}