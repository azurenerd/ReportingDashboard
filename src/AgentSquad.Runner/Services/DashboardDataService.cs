using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Configuration;

namespace AgentSquad.Runner.Services;

public class DashboardDataService : IDisposable
{
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IOptions<DashboardOptions> _options;
    private readonly IHostApplicationLifetime _hostLifetime;
    
    private DashboardData _cachedData;
    private string _lastError;
    private string _dataJsonPath;
    
    // FileSystemWatcher fields (Step 1 scaffolding)
    private FileSystemWatcher _fileSystemWatcher;
    private Timer _debounceTimer;
    private string _lastLoadedHash;
    private bool _isReloading;
    
    // Step 2: Debounce state tracking
    private bool _pendingReload;
    private object _debounceLock = new object();
    
    // Events
    public event Action OnDataChanged;
    
    // Properties
    public bool HasData => _cachedData != null && string.IsNullOrEmpty(_lastError);
    
    public DashboardDataService(
        ILogger<DashboardDataService> logger,
        IOptions<DashboardOptions> options,
        IHostApplicationLifetime hostLifetime)
    {
        _logger = logger;
        _options = options;
        _hostLifetime = hostLifetime;
        
        _dataJsonPath = Path.Combine(
            AppContext.BaseDirectory,
            _options.Value.DataJsonPath);
        
        _cachedData = null;
        _lastError = null;
        _lastLoadedHash = null;
        _isReloading = false;
        _pendingReload = false;
        
        // Load initial data on startup
        LoadData();
        
        // Initialize FileSystemWatcher after successful initial load
        InitializeFileWatch();
        
        _logger.LogInformation("DashboardDataService initialized");
    }
    
    /// <summary>
    /// Loads data from data.json on application startup.
    /// Called once in constructor before FileSystemWatcher initialization.
    /// </summary>
    private void LoadData()
    {
        try
        {
            if (!File.Exists(_dataJsonPath))
            {
                _lastError = $"data.json not found at {_dataJsonPath}";
                _logger.LogError(_lastError);
                _cachedData = null;
                return;
            }
            
            string json = File.ReadAllText(_dataJsonPath, Encoding.UTF8);
            _lastLoadedHash = ComputeFileHash(_dataJsonPath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = false,
                MaxDepth = 64
            };
            
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            
            // Post-deserialization validation
            ValidateData(data);
            
            _cachedData = data;
            _lastError = null;
            
            _logger.LogInformation("DashboardDataService initialized, data.json loaded successfully");
        }
        catch (JsonException ex)
        {
            _lastError = $"Failed to parse data.json: {ex.Message}";
            _logger.LogError(_lastError);
            _cachedData = null;
        }
        catch (IOException ex)
        {
            _lastError = $"IOException reading data.json: {ex.Message}";
            _logger.LogError(_lastError);
            _cachedData = null;
        }
        catch (Exception ex)
        {
            _lastError = $"Unexpected error loading data.json: {ex.Message}";
            _logger.LogError(ex, _lastError);
            _cachedData = null;
        }
    }
    
    /// <summary>
    /// Initializes FileSystemWatcher for data.json monitoring.
    /// Called from constructor after initial data load.
    /// Step 1 implementation; event handlers wired here, logic in Steps 2-4.
    /// </summary>
    private void InitializeFileWatch()
    {
        try
        {
            string directory = Path.GetDirectoryName(_dataJsonPath);
            string fileName = Path.GetFileName(_dataJsonPath);
            
            _fileSystemWatcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = false
            };
            
            // Wire up handlers (implementation in Step 2+)
            _fileSystemWatcher.Changed += OnFileChanged;
            _fileSystemWatcher.Created += OnFileChanged;
            _fileSystemWatcher.Deleted += OnFileChanged;
            _fileSystemWatcher.Error += OnFileWatcherError;
            
            // Enable raising events after handlers are wired
            _fileSystemWatcher.EnableRaisingEvents = true;
            
            _logger.LogInformation($"DashboardDataService file watch started on {_dataJsonPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FileSystemWatcher; file auto-update disabled");
            _fileSystemWatcher?.Dispose();
            _fileSystemWatcher = null;
        }
    }
    
    /// <summary>
    /// Step 2: File change event handler.
    /// Called by FileSystemWatcher on Changed/Created/Deleted events.
    /// Routes to ScheduleReload() to initiate debounce timer.
    /// </summary>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Ignore events for files other than data.json
        if (!e.Name.Equals(Path.GetFileName(_dataJsonPath), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        
        ScheduleReload();
    }
    
    /// <summary>
    /// Step 2: Schedules a reload with 500ms debounce.
    /// Cancels any pending timer and starts a new one.
    /// Multiple events within the window are coalesced into a single reload.
    /// </summary>
    private void ScheduleReload()
    {
        lock (_debounceLock)
        {
            // If a reload is already scheduled, log that this event is coalesced
            if (_pendingReload)
            {
                _logger.LogDebug("File event coalesced; debounce timer restarted");
            }
            else
            {
                _pendingReload = true;
                _logger.LogDebug("File event detected; scheduling reload with 500ms debounce");
            }
            
            // Cancel any pending timer
            if (_debounceTimer != null)
            {
                _debounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _debounceTimer.Dispose();
                _debounceTimer = null;
            }
            
            // Start new debounce timer (500ms from configuration)
            int debounceMs = _options.Value.FileWatchDebounceMs;
            _debounceTimer = new Timer(
                callback: OnDebounceTimerElapsed,
                state: null,
                dueTime: debounceMs,
                period: Timeout.Infinite);
        }
    }
    
    /// <summary>
    /// Step 2: Timer callback executed after debounce window (500ms) expires.
    /// Calls ReloadData() to re-read and re-parse data.json.
    /// Only executes if no new file events occurred during the debounce window.
    /// </summary>
    private void OnDebounceTimerElapsed(object state)
    {
        lock (_debounceLock)
        {
            // Timer has elapsed; reset pending reload flag
            _pendingReload = false;
            
            // Dispose the timer
            if (_debounceTimer != null)
            {
                _debounceTimer.Dispose();
                _debounceTimer = null;
            }
        }
        
        // Call ReloadData() to re-read and re-parse data.json
        // Step 3 implements hash checking; Step 4 implements full reload logic
        ReloadData();
    }
    
    /// <summary>
    /// Step 3-4: Reloads data from data.json file.
    /// Implements hash-based duplicate detection (Step 3) and full reload logic (Step 4).
    /// Checks file hash before parsing to prevent redundant processing.
    /// </summary>
    private void ReloadData()
    {
        _logger.LogInformation("data.json change detected, reloading...");
        
        // Step 3: Hash-based duplicate detection
        // Check if file content has actually changed
        if (!File.Exists(_dataJsonPath))
        {
            _lastError = $"data.json not found at {_dataJsonPath}";
            _logger.LogError(_lastError);
            _cachedData = null;
            OnDataChanged?.Invoke();
            return;
        }
        
        try
        {
            // Compute current file hash
            string currentHash = ComputeFileHash(_dataJsonPath);
            
            // Step 3: Compare with last loaded hash
            if (currentHash == _lastLoadedHash)
            {
                _logger.LogDebug("File unchanged; skipping reload");
                return;
            }
            
            // Step 4: Hash differs; proceed with reload
            _logger.LogDebug("File hash differs from last load; proceeding with reload");
            
            // Re-read file content
            string json = File.ReadAllText(_dataJsonPath, Encoding.UTF8);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = false,
                MaxDepth = 64
            };
            
            // Re-parse JSON
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            
            // Re-validate
            ValidateData(data);
            
            // Update cache and hash on success
            _cachedData = data;
            _lastLoadedHash = currentHash;
            _lastError = null;
            
            _logger.LogInformation("data.json reloaded successfully");
            
            // Notify subscribers
            OnDataChanged?.Invoke();
        }
        catch (JsonException ex)
        {
            _lastError = $"Failed to parse data.json: {ex.Message}";
            _logger.LogError(_lastError);
            // Keep last-known-good data; don't update cache
            
            // Still notify subscribers so UI can show error state
            OnDataChanged?.Invoke();
        }
        catch (IOException ex)
        {
            _lastError = $"IOException reading data.json: {ex.Message}";
            _logger.LogError(_lastError);
            // Keep last-known-good data
            
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _lastError = $"Unexpected error reloading data.json: {ex.Message}";
            _logger.LogError(ex, _lastError);
            // Keep last-known-good data
            
            OnDataChanged?.Invoke();
        }
    }
    
    /// <summary>
    /// Computes SHA256 hash of file content to detect unchanged files.
    /// Used by duplicate detection logic (Step 3).
    /// </summary>
    private string ComputeFileHash(string filePath)
    {
        try
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                return Convert.ToHexString(hashBytes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to compute hash for {filePath}");
            return null;
        }
    }
    
    /// <summary>
    /// Validates deserialized DashboardData against schema constraints.
    /// </summary>
    private void ValidateData(DashboardData data)
    {
        if (data?.Project == null)
        {
            throw new InvalidOperationException("Project is required in data.json");
        }
        
        if (string.IsNullOrWhiteSpace(data.Project.Name))
        {
            throw new InvalidOperationException("Project.Name is required");
        }
        
        if (data.Project.Name.Length > 256)
        {
            throw new InvalidOperationException("Project.Name exceeds max length of 256");
        }
        
        if (data.Milestones != null)
        {
            foreach (var milestone in data.Milestones)
            {
                if (string.IsNullOrWhiteSpace(milestone.Name))
                {
                    throw new InvalidOperationException("Milestone.Name is required");
                }
                
                if (milestone.Name.Length > 256)
                {
                    throw new InvalidOperationException("Milestone.Name exceeds max length of 256");
                }
                
                if (string.IsNullOrWhiteSpace(milestone.Status) || 
                    !IsValidMilestoneStatus(milestone.Status))
                {
                    throw new InvalidOperationException(
                        $"Milestone.Status must be 'Completed', 'On Track', or 'At Risk'; got '{milestone.Status}'");
                }
            }
        }
        
        if (data.WorkItems != null)
        {
            foreach (var item in data.WorkItems)
            {
                if (string.IsNullOrWhiteSpace(item.Title))
                {
                    throw new InvalidOperationException("WorkItem.Title is required");
                }
                
                if (item.Title.Length > 512)
                {
                    throw new InvalidOperationException("WorkItem.Title exceeds max length of 512");
                }
                
                if (!Enum.IsDefined(typeof(WorkItemStatus), item.Status))
                {
                    throw new InvalidOperationException(
                        $"WorkItem.Status must be Shipped, InProgress, or CarriedOver; got '{item.Status}'");
                }
                
                if (item.Assignee?.Length > 256)
                {
                    throw new InvalidOperationException("WorkItem.Assignee exceeds max length of 256");
                }
            }
        }
    }
    
    /// <summary>
    /// Validates milestone status enum values.
    /// </summary>
    private bool IsValidMilestoneStatus(string status)
    {
        return status == "Completed" || status == "On Track" || status == "At Risk";
    }
    
    // Public accessor methods (from architecture spec)
    
    public DashboardData GetCurrentData() => _cachedData;
    
    public Project GetProject() => _cachedData?.Project;
    
    public IReadOnlyList<Milestone> GetMilestones()
    {
        if (_cachedData?.Milestones == null)
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
        if (_cachedData?.WorkItems == null)
        {
            return new List<WorkItem>().AsReadOnly();
        }
        
        return _cachedData.WorkItems.ToList().AsReadOnly();
    }
    
    public IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status)
    {
        if (_cachedData?.WorkItems == null)
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
        if (_cachedData?.WorkItems == null)
        {
            return (0, 0, 0);
        }
        
        int shipped = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.Shipped);
        int inProgress = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.InProgress);
        int carriedOver = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.CarriedOver);
        
        return (shipped, inProgress, carriedOver);
    }
    
    public string GetLastError() => _lastError;
    
    /// <summary>
    /// Error handler for FileSystemWatcher exceptions.
    /// </summary>
    private void OnFileWatcherError(object sender, ErrorEventArgs e)
    {
        Exception ex = e.GetException();
        _logger.LogError(ex, "FileSystemWatcher error; file auto-update may be disabled");
    }
    
    // IDisposable implementation
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_debounceLock)
            {
                // Cancel pending debounce timer
                if (_debounceTimer != null)
                {
                    _debounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _debounceTimer.Dispose();
                    _debounceTimer = null;
                }
            }
            
            // Stop and dispose FileSystemWatcher
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Changed -= OnFileChanged;
                _fileSystemWatcher.Created -= OnFileChanged;
                _fileSystemWatcher.Deleted -= OnFileChanged;
                _fileSystemWatcher.Error -= OnFileWatcherError;
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
            
            _logger.LogInformation("DashboardDataService disposed; file watch stopped");
        }
    }
    
    ~DashboardDataService()
    {
        Dispose(false);
    }
}