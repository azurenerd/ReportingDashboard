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
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        // No resources to dispose yet - T3 adds FileSystemWatcher
    }
}