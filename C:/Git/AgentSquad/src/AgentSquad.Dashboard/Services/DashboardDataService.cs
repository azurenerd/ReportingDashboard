using System.Text.Json;
using System.Text.Json.Serialization;
using AgentSquad.Dashboard.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Dashboard.Services;

public interface IDashboardDataService
{
    DashboardData CurrentData { get; }
    bool IsValid { get; }
    string? LastErrorMessage { get; }
    event EventHandler? DataRefreshed;
    Task InitializeAsync();
    Task RefreshAsync();
    Task<bool> IsDataValidAsync();
}

public class DashboardDataService : IDashboardDataService, IHostedService, IAsyncDisposable
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;
    private string _dataPath = string.Empty;
    private FileSystemWatcher? _fileWatcher;
    private System.Timers.Timer? _pollTimer;
    private DashboardData _currentData = new();
    private DashboardData _previousData = new();
    private bool _isValid;
    private string? _lastErrorMessage;
    private string _lastFileHash = string.Empty;

    public DashboardData CurrentData => _currentData;
    public bool IsValid => _isValid;
    public string? LastErrorMessage => _lastErrorMessage;
    public event EventHandler? DataRefreshed;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync();
        return;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _fileWatcher?.Dispose();
        _pollTimer?.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        _dataPath = Path.Combine(_env.WebRootPath, "data.json");
        
        if (!File.Exists(_dataPath))
        {
            _logger.LogWarning("data.json not found at {Path}. Creating default...", _dataPath);
            await CreateDefaultDataAsync();
        }

        await RefreshAsync();

        InitializeFileWatcher();
        InitializePollingFallback();
        
        _logger.LogInformation("DashboardDataService initialized. DataPath: {Path}", _dataPath);
    }

    private async Task CreateDefaultDataAsync()
    {
        var defaultData = new DashboardData
        {
            ProjectName = "My Project",
            ProjectId = "PROJ-001",
            LastUpdated = DateTime.UtcNow,
            Milestones = new(),
            Completed = new(),
            InProgress = new(),
            CarriedOver = new()
        };

        var json = JsonSerializer.Serialize(defaultData, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_dataPath, json);
    }

    public async Task RefreshAsync()
    {
        try
        {
            if (!File.Exists(_dataPath))
            {
                _isValid = false;
                _lastErrorMessage = "data.json file not found";
                _logger.LogError("File not found: {Path}", _dataPath);
                DataRefreshed?.Invoke(this, EventArgs.Empty);
                return;
            }

            var currentHash = ComputeHash(await File.ReadAllBytesAsync(_dataPath));
            if (currentHash == _lastFileHash)
            {
                return; // No changes
            }

            _lastFileHash = currentHash;
            var json = await File.ReadAllTextAsync(_dataPath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var deserialized = JsonSerializer.Deserialize<DashboardData>(json, options);
            if (deserialized == null)
            {
                throw new InvalidOperationException("Deserialization returned null");
            }

            if (!await IsDataValidAsync())
            {
                _isValid = false;
                _lastErrorMessage = "Data validation failed";
                _logger.LogWarning("Data validation failed for {Path}", _dataPath);
            }
            else
            {
                _previousData = _currentData;
                _currentData = deserialized;
                _isValid = true;
                _lastErrorMessage = null;
                _logger.LogInformation("Data refreshed: {Items} total items", _currentData.TotalCount);
            }
        }
        catch (JsonException ex)
        {
            _isValid = false;
            _lastErrorMessage = $"Invalid JSON: {ex.Message}";
            _logger.LogError(ex, "JSON parse error in data.json");
        }
        catch (Exception ex)
        {
            _isValid = false;
            _lastErrorMessage = $"Error: {ex.Message}";
            _logger.LogError(ex, "Unexpected error during refresh");
        }
        finally
        {
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task<bool> IsDataValidAsync()
    {
        if (_currentData == null) return false;
        
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(_currentData.ProjectName))
            errors.Add("ProjectName required");
        if (string.IsNullOrWhiteSpace(_currentData.ProjectId))
            errors.Add("ProjectId required");
        if (_currentData.Completed == null)
            errors.Add("Completed array required");
        if (_currentData.InProgress == null)
            errors.Add("InProgress array required");
        if (_currentData.CarriedOver == null)
            errors.Add("CarriedOver array required");

        if (errors.Any())
        {
            _logger.LogWarning("Validation errors: {Errors}", string.Join(", ", errors));
            return false;
        }

        return true;
    }

    private void InitializeFileWatcher()
    {
        var directory = Path.GetDirectoryName(_dataPath) ?? _env.WebRootPath;
        var fileName = Path.GetFileName(_dataPath);

        _fileWatcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _fileWatcher.Changed += async (s, e) =>
        {
            _logger.LogDebug("File change detected: {Name}", e.Name);
            await RefreshAsync();
        };

        _fileWatcher.Error += (s, e) =>
        {
            _logger.LogError("FileSystemWatcher error: {Error}", e.GetException()?.Message);
        };
    }

    private void InitializePollingFallback()
    {
        _pollTimer = new System.Timers.Timer(30000); // 30 seconds
        _pollTimer.Elapsed += async (s, e) => await RefreshAsync();
        _pollTimer.AutoReset = true;
        _pollTimer.Start();
    }

    private string ComputeHash(byte[] data)
    {
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            return Convert.ToBase64String(sha.ComputeHash(data));
        }
    }

    public async ValueTask DisposeAsync()
    {
        _fileWatcher?.Dispose();
        _pollTimer?.Dispose();
        await Task.CompletedTask;
    }
}