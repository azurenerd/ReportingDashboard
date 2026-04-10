using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDashboardDataService
{
    private readonly string _filePath;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DashboardData? Data { get; private set; }
    public string? LoadError { get; private set; }
    public bool IsLoaded => Data != null;
    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        _filePath = configuration.GetValue<string>("DashboardDataPath")
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "data.json");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        _logger.LogInformation("DashboardDataService initialized. Data file path: {FilePath}", _filePath);
    }

    public async Task LoadAsync()
    {
        _logger.LogInformation("Loading dashboard data from {FilePath}", _filePath);

        if (!File.Exists(_filePath))
        {
            LoadError = $"Dashboard data file not found at: {_filePath}";
            _logger.LogError("Dashboard data file not found at: {FilePath}", _filePath);
            OnDataChanged?.Invoke();
            return;
        }

        var json = await ReadFileWithRetryAsync();
        if (json is null)
        {
            LoadError = $"Unable to read dashboard data file at: {_filePath}. The file may be locked by another process.";
            _logger.LogError("All retry attempts exhausted reading {FilePath}", _filePath);
            OnDataChanged?.Invoke();
            return;
        }

        try
        {
            var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);

            if (data?.Project is null)
            {
                LoadError = "Invalid data: missing required 'project' section";
                _logger.LogError("Deserialized data is missing required 'project' section");
                OnDataChanged?.Invoke();
                return;
            }

            Data = data;
            LoadError = null;
            _logger.LogInformation("Dashboard data loaded successfully for project: {ProjectName}", Data.Project.Name);
        }
        catch (JsonException ex)
        {
            LoadError = $"Error loading data: {ex.Message}";
            _logger.LogError(ex, "Failed to deserialize dashboard data from {FilePath}", _filePath);
            // Retain previous valid Data if it exists
        }

        OnDataChanged?.Invoke();
    }

    private async Task<string?> ReadFileWithRetryAsync()
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (IOException ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "File read attempt {Attempt}/3 failed for {Path}. Retrying in 200ms.", attempt, _filePath);
                await Task.Delay(200);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "File read attempt {Attempt}/3 failed for {Path}. No more retries.", attempt, _filePath);
            }
        }

        return null;
    }

    public void Dispose()
    {
        // No resources to dispose yet - T3 adds FileSystemWatcher
    }
}