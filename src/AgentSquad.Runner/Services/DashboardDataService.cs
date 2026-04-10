using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class DashboardDataService : IDashboardDataService, IDisposable
{
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IOptions<DashboardOptions> _options;
    private DashboardData? _cachedData;
    private string? _lastError;
    private string? _lastLoadedHash;
    private FileSystemWatcher? _fileWatcher;
    private System.Threading.Timer? _debounceTimer;
    private bool _disposed;

    public event Action? OnDataChanged;

    public DashboardDataService(
        ILogger<DashboardDataService> logger,
        IOptions<DashboardOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        LoadData();
        InitializeFileWatcher();
    }

    public DashboardData? GetCurrentData() => _cachedData;

    public Project? GetProject() => _cachedData?.Project;

    public IReadOnlyList<Milestone> GetMilestones()
    {
        if (_cachedData?.Milestones == null || _cachedData.Milestones.Count == 0)
        {
            return new List<Milestone>().AsReadOnly();
        }

        return _cachedData.Milestones
            .OrderBy(m => m.Date)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<WorkItem> GetWorkItems()
    {
        if (_cachedData?.WorkItems == null || _cachedData.WorkItems.Count == 0)
        {
            return new List<WorkItem>().AsReadOnly();
        }

        return _cachedData.WorkItems.AsReadOnly();
    }

    public IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status)
    {
        if (_cachedData?.WorkItems == null || _cachedData.WorkItems.Count == 0)
        {
            return new List<WorkItem>().AsReadOnly();
        }

        return _cachedData.WorkItems
            .Where(w => w.Status == status)
            .ToList()
            .AsReadOnly();
    }

    public (int Shipped, int InProgress, int CarriedOver) GetStatusCounts()
    {
        if (_cachedData?.WorkItems == null || _cachedData.WorkItems.Count == 0)
        {
            return (0, 0, 0);
        }

        int shipped = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.Shipped);
        int inProgress = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.InProgress);
        int carriedOver = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.CarriedOver);

        return (shipped, inProgress, carriedOver);
    }

    public string? GetLastError() => _lastError;

    public bool HasData => _cachedData != null;

    private void LoadData()
    {
        try
        {
            string dataJsonPath = ResolveDataJsonPath();

            if (!File.Exists(dataJsonPath))
            {
                SetError($"data.json not found at {dataJsonPath}. Please create the file or update the DataJsonPath in appsettings.json.");
                _logger.LogError($"Data file not found at path: {dataJsonPath}");
                return;
            }

            string json = ReadFileWithRetry(dataJsonPath);
            LoadFromJson(json);
            _logger.LogInformation($"DashboardDataService initialized successfully. Data loaded from {dataJsonPath}");
        }
        catch (Exception ex)
        {
            SetError($"Unexpected error loading dashboard data: {ex.Message}");
            _logger.LogError(ex, "Unexpected error in DashboardDataService initialization");
        }
    }

    private void LoadFromJson(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                SetError("data.json file is empty. Please provide valid JSON content.");
                _logger.LogError("data.json file is empty");
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = false,
                MaxDepth = 64
            };

            DashboardData? data = JsonSerializer.Deserialize<DashboardData>(json, options);

            ValidateData(data);

            _cachedData = data;
            _lastError = null;
            _lastLoadedHash = ComputeHash(json);
            _logger.LogInformation("Dashboard data loaded and validated successfully");
        }
        catch (JsonException ex)
        {
            SetError($"JSON syntax error in data.json: {ex.Message}");
            _logger.LogError(ex, "JSON parsing error in data.json");
        }
        catch (ValidationException ex)
        {
            SetError($"Data validation error: {ex.Message}");
            _logger.LogError(ex, "Data validation failed");
        }
    }

    private void ValidateData(DashboardData? data)
    {
        if (data == null)
        {
            throw new ValidationException("Parsed data is null. Please provide valid JSON matching the expected schema.");
        }

        var context = new ValidationContext(data);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(data, context, results, validateAllProperties: true);

        if (!isValid)
        {
            var errorMessages = results
                .Select(r => r.ErrorMessage ?? "Validation failed with no error message")
                .ToList();

            string combinedError = string.Join(" | ", errorMessages);
            throw new ValidationException($"Data validation failed: {combinedError}");
        }
    }

    private string ReadFileWithRetry(string path, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch (IOException ex) when (i < maxRetries - 1)
            {
                _logger.LogWarning($"File locked; retrying in 100ms ({i + 1}/{maxRetries})");
                Thread.Sleep(100);
            }
            catch (IOException ex) when (i == maxRetries - 1)
            {
                throw new IOException($"Could not read {path} after {maxRetries} retries: {ex.Message}", ex);
            }
        }

        throw new IOException($"Could not read {path} after {maxRetries} retries");
    }

    private string ResolveDataJsonPath()
    {
        string? configuredPath = _options.Value?.DataJsonPath;

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            configuredPath = "data.json";
        }

        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.Combine(AppContext.BaseDirectory, configuredPath);
    }

    private string ComputeHash(string content)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(hashBytes);
        }
    }

    private void SetError(string message)
    {
        _lastError = message;
        _cachedData = null;
    }

    private void InitializeFileWatcher()
    {
        try
        {
            string dataJsonPath = ResolveDataJsonPath();
            string? directory = Path.GetDirectoryName(dataJsonPath);
            string? fileName = Path.GetFileName(dataJsonPath);

            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            {
                _logger.LogWarning("Could not initialize file watcher: invalid path");
                return;
            }

            _fileWatcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnFileChanged;
            _logger.LogInformation($"File watcher initialized for {dataJsonPath}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize file watcher");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new System.Threading.Timer(
            _ => ReloadData(),
            null,
            _options.Value?.FileWatchDebounceMs ?? 500,
            System.Threading.Timeout.Infinite);
    }

    private void ReloadData()
    {
        try
        {
            string dataJsonPath = ResolveDataJsonPath();
            string json = ReadFileWithRetry(dataJsonPath);
            string newHash = ComputeHash(json);

            if (newHash != _lastLoadedHash)
            {
                LoadFromJson(json);
                OnDataChanged?.Invoke();
                _logger.LogInformation("Dashboard data reloaded from file system watcher");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading dashboard data from file watcher");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _fileWatcher?.Dispose();
        _debounceTimer?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}