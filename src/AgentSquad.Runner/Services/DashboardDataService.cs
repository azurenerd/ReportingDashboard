using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Options;

namespace AgentSquad.Runner.Services;

public class DashboardDataService : IDisposable
{
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IOptions<DashboardOptions> _options;
    private readonly FileSystemWatcher? _watcher;
    private DashboardData? _cachedData;
    private string? _lastLoadedHash;
    private string? _lastError;
    private Timer? _debounceTimer;
    private bool _isDisposed;

    public event Action? OnDataChanged;

    public bool HasData => _cachedData != null && string.IsNullOrEmpty(_lastError);

    public DashboardDataService(ILogger<DashboardDataService> logger, IOptions<DashboardOptions> options)
    {
        _logger = logger;
        _options = options;

        try
        {
            LoadDataFromFile();
            InitializeFileWatcher();
            _logger.LogInformation("DashboardDataService initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing DashboardDataService");
            _lastError = $"Failed to initialize dashboard: {ex.Message}";
        }
    }

    private void LoadDataFromFile()
    {
        try
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, _options.Value.DataJsonPath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"data.json not found at {filePath}");
            }

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var fileHash = ComputeHash(json);

            if (fileHash == _lastLoadedHash)
            {
                _logger.LogDebug("File hash unchanged; skipping reload");
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = false,
                MaxDepth = 64
            };

            _cachedData = JsonSerializer.Deserialize<DashboardData>(json, options)
                ?? throw new JsonException("Failed to deserialize data.json");

            ValidateData(_cachedData);
            _lastLoadedHash = fileHash;
            _lastError = null;

            _logger.LogInformation("data.json loaded and parsed successfully");
        }
        catch (JsonException ex)
        {
            _lastError = $"JSON parsing error: {ex.Message}";
            _logger.LogError(ex, "Failed to parse data.json");
        }
        catch (IOException ex)
        {
            _lastError = $"File read error: {ex.Message}";
            _logger.LogError(ex, "IOException reading data.json");
        }
        catch (Exception ex)
        {
            _lastError = $"Unexpected error: {ex.Message}";
            _logger.LogError(ex, "Unexpected error loading data.json");
        }
    }

    private void InitializeFileWatcher()
    {
        try
        {
            var directory = AppContext.BaseDirectory;
            var fileName = _options.Value.DataJsonPath;

            _watcher = new FileSystemWatcher(directory)
            {
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _logger.LogInformation("FileSystemWatcher initialized for data.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing FileSystemWatcher");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(
                _ => ReloadData(),
                null,
                _options.Value.FileWatchDebounceMs,
                Timeout.Infinite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnFileChanged");
        }
    }

    private void ReloadData()
    {
        try
        {
            _debounceTimer?.Dispose();
            LoadDataFromFile();
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading data");
        }
    }

    private void ValidateData(DashboardData data)
    {
        if (data?.Project == null)
            throw new InvalidOperationException("Project is required");

        if (string.IsNullOrEmpty(data.Project.Name))
            throw new InvalidOperationException("Project name is required");

        if (data.Milestones?.Any(m => string.IsNullOrEmpty(m.Name)) == true)
            throw new InvalidOperationException("Milestone name is required");

        if (data.WorkItems?.Any(w => !Enum.IsDefined(w.Status)) == true)
            throw new InvalidOperationException("Invalid work item status");
    }

    private string ComputeHash(string input)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }

    public DashboardData? GetCurrentData() => _cachedData;

    public Project? GetProject() => _cachedData?.Project;

    public IReadOnlyList<Milestone> GetMilestones() =>
        _cachedData?.Milestones?.OrderBy(m => m.Date).ToList().AsReadOnly()
        ?? new List<Milestone>().AsReadOnly();

    public IReadOnlyList<WorkItem> GetWorkItems() =>
        _cachedData?.WorkItems?.AsReadOnly() ?? new List<WorkItem>().AsReadOnly();

    public IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status) =>
        _cachedData?.WorkItems?
            .Where(w => w.Status == status)
            .ToList()
            .AsReadOnly() ?? new List<WorkItem>().AsReadOnly();

    public (int Shipped, int InProgress, int CarriedOver) GetStatusCounts()
    {
        var items = _cachedData?.WorkItems ?? new List<WorkItem>();
        return (
            items.Count(w => w.Status == WorkItemStatus.Shipped),
            items.Count(w => w.Status == WorkItemStatus.InProgress),
            items.Count(w => w.Status == WorkItemStatus.CarriedOver)
        );
    }

    public string? GetLastError() => _lastError;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _watcher?.Dispose();
        _debounceTimer?.Dispose();
        _isDisposed = true;
    }
}