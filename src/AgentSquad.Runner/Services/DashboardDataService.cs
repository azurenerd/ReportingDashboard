using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentSquad.Runner.Services;

public class DashboardDataService : IDashboardDataService, IDisposable
{
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IOptions<DashboardOptions> _options;
    
    private DashboardData _cachedData;
    private string _lastError;
    private string _lastLoadedHash;
    private FileSystemWatcher _watcher;
    private Timer _debounceTimer;
    private bool _disposed;
    private readonly object _lockObject = new();

    public event Action OnDataChanged;

    public bool HasData { get; private set; }

    public DashboardDataService(
        ILogger<DashboardDataService> logger,
        IOptions<DashboardOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        
        _cachedData = new DashboardData();
        _lastError = null;
        _lastLoadedHash = null;
        HasData = false;

        LoadFromJson();
        InitializeFileWatcher();
    }

    public DashboardData GetCurrentData()
    {
        return _cachedData;
    }

    public Project GetProject()
    {
        return _cachedData?.Project;
    }

    public IReadOnlyList<Milestone> GetMilestones()
    {
        if (_cachedData?.Milestones == null)
            return new List<Milestone>().AsReadOnly();
        
        return _cachedData.Milestones
            .OrderBy(m => m.Date)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<WorkItem> GetWorkItems()
    {
        if (_cachedData?.WorkItems == null)
            return new List<WorkItem>().AsReadOnly();
        
        return _cachedData.WorkItems.AsReadOnly();
    }

    public IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status)
    {
        throw new NotImplementedException();
    }

    public (int Shipped, int InProgress, int CarriedOver) GetStatusCounts()
    {
        throw new NotImplementedException();
    }

    public string GetLastError()
    {
        return _lastError;
    }

    private void InitializeFileWatcher()
    {
        try
        {
            var dataJsonPath = Path.Combine(
                AppContext.BaseDirectory,
                _options.Value?.DataJsonPath ?? "data.json");

            var directoryPath = Path.GetDirectoryName(dataJsonPath);
            var fileName = Path.GetFileName(dataJsonPath);

            _watcher = new FileSystemWatcher(directoryPath, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = false
            };

            _watcher.Changed += OnDataJsonChanged;
            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("FileSystemWatcher initialized for data.json at {path}", dataJsonPath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to initialize FileSystemWatcher: {error}", ex.Message);
        }
    }

    private void OnDataJsonChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lockObject)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(
                _ => ReloadDataFromFile(),
                null,
                _options.Value?.FileWatchDebounceMs ?? 500,
                Timeout.Infinite);
        }
    }

    private void ReloadDataFromFile()
    {
        try
        {
            lock (_lockObject)
            {
                var dataJsonPath = Path.Combine(
                    AppContext.BaseDirectory,
                    _options.Value?.DataJsonPath ?? "data.json");

                if (!File.Exists(dataJsonPath))
                {
                    _logger.LogInformation("data.json change detected, re-parsing...");
                    LoadFromJson();
                    OnDataChanged?.Invoke();
                    return;
                }

                var fileHash = HashFile(dataJsonPath);
                
                if (fileHash == _lastLoadedHash)
                {
                    _logger.LogDebug("File hash unchanged; skipping reload");
                    return;
                }

                _logger.LogInformation("data.json change detected, re-parsing...");
                LoadFromJson();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error reloading data from file: {error}", ex.Message);
        }
        finally
        {
            lock (_lockObject)
            {
                _debounceTimer?.Dispose();
                _debounceTimer = null;
            }
        }
    }

    private string HashFile(string filePath)
    {
        try
        {
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    var hashBytes = sha256.ComputeHash(fileStream);
                    return Convert.ToBase64String(hashBytes);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error computing file hash: {error}", ex.Message);
            return null;
        }
    }

    private void LoadFromJson()
    {
        try
        {
            var dataJsonPath = Path.Combine(
                AppContext.BaseDirectory,
                _options.Value?.DataJsonPath ?? "data.json");

            if (!File.Exists(dataJsonPath))
            {
                throw new FileNotFoundException($"data.json not found at {dataJsonPath}");
            }

            var jsonContent = File.ReadAllText(dataJsonPath, System.Text.Encoding.UTF8);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                AllowTrailingCommas = false,
                MaxDepth = 64
            };

            var data = JsonSerializer.Deserialize<DashboardData>(jsonContent, options);

            ValidateData(data);

            _cachedData = data;
            _lastError = null;
            HasData = true;

            var fileHash = HashFile(dataJsonPath);
            _lastLoadedHash = fileHash;

            _logger.LogInformation("DashboardDataService initialized, data.json loaded successfully");
        }
        catch (FileNotFoundException ex)
        {
            _lastError = $"FileNotFound: {ex.Message}";
            _logger.LogError("Failed to load data.json: {error}", _lastError);
            HasData = false;
        }
        catch (JsonException ex)
        {
            _lastError = $"JSON syntax error in data.json: {ex.Message}";
            _logger.LogError("Failed to parse data.json: {error}", _lastError);
            HasData = false;
        }
        catch (ArgumentException ex)
        {
            _lastError = $"Validation error: {ex.Message}";
            _logger.LogError("Validation failed for data.json: {error}", _lastError);
            HasData = false;
        }
        catch (IOException ex)
        {
            _lastError = $"IO error reading data.json: {ex.Message}";
            _logger.LogError("IO error reading data.json: {error}", _lastError);
            HasData = false;
        }
        catch (Exception ex)
        {
            _lastError = $"Unexpected error loading data.json: {ex.Message}";
            _logger.LogError("Unexpected error: {error}", _lastError);
            HasData = false;
        }
    }

    private void ValidateData(DashboardData data)
    {
        if (data == null)
        {
            throw new ArgumentException("Deserialized data is null");
        }

        if (data.Project == null)
        {
            throw new ArgumentException("Project is required in data.json");
        }

        if (string.IsNullOrWhiteSpace(data.Project.Name))
        {
            throw new ArgumentException("Project.Name is required and cannot be empty");
        }

        if (data.Project.Name.Length > 256)
        {
            throw new ArgumentException($"Project.Name exceeds maximum length of 256 characters (current: {data.Project.Name.Length})");
        }

        if (!string.IsNullOrEmpty(data.Project.Description) && data.Project.Description.Length > 1024)
        {
            throw new ArgumentException($"Project.Description exceeds maximum length of 1024 characters (current: {data.Project.Description.Length})");
        }

        if (data.Milestones != null)
        {
            foreach (var milestone in data.Milestones)
            {
                if (string.IsNullOrWhiteSpace(milestone.Name))
                {
                    throw new ArgumentException("Milestone.Name is required and cannot be empty");
                }

                if (milestone.Name.Length > 256)
                {
                    throw new ArgumentException($"Milestone.Name exceeds maximum length of 256 characters (current: {milestone.Name.Length})");
                }

                if (string.IsNullOrEmpty(milestone.Status))
                {
                    throw new ArgumentException("Milestone.Status is required");
                }

                var validStatuses = new[] { "Completed", "On Track", "At Risk" };
                if (!validStatuses.Contains(milestone.Status))
                {
                    throw new ArgumentException($"Invalid Milestone.Status '{milestone.Status}'. Valid values: {string.Join(", ", validStatuses)}");
                }
            }
        }

        if (data.WorkItems != null)
        {
            foreach (var workItem in data.WorkItems)
            {
                if (string.IsNullOrWhiteSpace(workItem.Title))
                {
                    throw new ArgumentException("WorkItem.Title is required and cannot be empty");
                }

                if (workItem.Title.Length > 512)
                {
                    throw new ArgumentException($"WorkItem.Title exceeds maximum length of 512 characters (current: {workItem.Title.Length})");
                }

                if (!Enum.IsDefined(typeof(WorkItemStatus), workItem.Status))
                {
                    throw new ArgumentException($"Invalid WorkItem.Status '{workItem.Status}'. Valid values: {string.Join(", ", Enum.GetNames(typeof(WorkItemStatus)))}");
                }

                if (!string.IsNullOrEmpty(workItem.Assignee) && workItem.Assignee.Length > 256)
                {
                    throw new ArgumentException($"WorkItem.Assignee exceeds maximum length of 256 characters (current: {workItem.Assignee.Length})");
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _debounceTimer?.Dispose();
        _watcher?.Dispose();
        _disposed = true;
    }
}