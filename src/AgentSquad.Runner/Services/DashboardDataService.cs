using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class DashboardDataService : IDashboardDataService, IDisposable
    {
        private readonly ILogger<DashboardDataService> _logger;
        private readonly IOptions<DashboardOptions> _options;
        private DashboardData _cachedData;
        private string _lastError;
        private string _lastLoadedHash;
        private FileSystemWatcher _watcher;
        private Timer _debounceTimer;
        private bool _pendingReload;
        private object _lockObject = new object();
        private bool _disposed;

        public event Action OnDataChanged;

        public bool HasData => _cachedData != null && string.IsNullOrEmpty(_lastError);

        public DashboardDataService(ILogger<DashboardDataService> logger, IOptions<DashboardOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _pendingReload = false;

            LoadInitialData();
            InitializeFileWatcher();
        }

        private void LoadInitialData()
        {
            try
            {
                _cachedData = LoadFromJson();
                _lastError = null;
                _lastLoadedHash = HashFile(_options.Value.DataJsonPath);
                _logger.LogInformation("DashboardDataService initialized, data.json loaded successfully");
            }
            catch (FileNotFoundException ex)
            {
                _lastError = $"File not found: {ex.Message}";
                _logger.LogError($"IOException reading data.json: {_lastError}");
                _cachedData = null;
            }
            catch (JsonException ex)
            {
                _lastError = $"JSON syntax error in data.json: {ex.Message}";
                _logger.LogError($"Failed to parse data.json: {_lastError}");
                _cachedData = null;
            }
            catch (Exception ex)
            {
                _lastError = $"Error loading data.json: {ex.Message}";
                _logger.LogError($"Error loading data.json: {_lastError}");
                _cachedData = null;
            }
        }

        private void InitializeFileWatcher()
        {
            try
            {
                var dataJsonPath = Path.Combine(AppContext.BaseDirectory, _options.Value.DataJsonPath);
                var directoryPath = Path.GetDirectoryName(dataJsonPath);
                var fileName = Path.GetFileName(dataJsonPath);

                _watcher = new FileSystemWatcher(directoryPath, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _watcher.Changed += OnFileChanged;
                _watcher.EnableRaisingEvents = true;

                _logger.LogInformation("FileSystemWatcher initialized for data.json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize FileSystemWatcher: {ex.Message}");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            lock (_lockObject)
            {
                _pendingReload = true;
                _debounceTimer?.Dispose();
                _debounceTimer = new Timer(_ => OnDebounceTimerElapsed(), null, _options.Value.FileWatchDebounceMs, Timeout.Infinite);
            }
        }

        private void OnDebounceTimerElapsed()
        {
            lock (_lockObject)
            {
                if (!_pendingReload)
                    return;

                _pendingReload = false;

                try
                {
                    var dataJsonPath = Path.Combine(AppContext.BaseDirectory, _options.Value.DataJsonPath);
                    var fileHash = HashFile(dataJsonPath);

                    if (fileHash == _lastLoadedHash)
                    {
                        _logger.LogDebug("File hash unchanged; skipping reload");
                        return;
                    }

                    _logger.LogInformation("data.json change detected, re-parsing...");

                    var newData = LoadFromJson();
                    _cachedData = newData;
                    _lastError = null;
                    _lastLoadedHash = fileHash;

                    OnDataChanged?.Invoke();
                }
                catch (JsonException ex)
                {
                    _lastError = $"JSON syntax error in data.json: {ex.Message}";
                    _logger.LogError($"Failed to parse data.json: {_lastError}");
                    OnDataChanged?.Invoke();
                }
                catch (IOException ex)
                {
                    _lastError = $"IOException reading data.json: {ex.Message}";
                    _logger.LogError(_lastError);
                    OnDataChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    _lastError = $"Error loading data.json: {ex.Message}";
                    _logger.LogError(_lastError);
                    OnDataChanged?.Invoke();
                }
            }
        }

        private DashboardData LoadFromJson()
        {
            var dataJsonPath = Path.Combine(AppContext.BaseDirectory, _options.Value.DataJsonPath);

            if (!File.Exists(dataJsonPath))
            {
                throw new FileNotFoundException($"data.json not found at {dataJsonPath}");
            }

            var json = File.ReadAllText(dataJsonPath, Encoding.UTF8);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = false,
                MaxDepth = 64,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var data = JsonSerializer.Deserialize<DashboardData>(json, options);

            ValidateData(data);

            return data;
        }

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
                throw new InvalidOperationException("Project.Name exceeds 256 character limit");
            }

            if (!string.IsNullOrEmpty(data.Project.Description) && data.Project.Description.Length > 1024)
            {
                throw new InvalidOperationException("Project.Description exceeds 1024 character limit");
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
                        throw new InvalidOperationException("Milestone.Name exceeds 256 character limit");
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
                        throw new InvalidOperationException("WorkItem.Title exceeds 512 character limit");
                    }

                    if (!Enum.IsDefined(typeof(WorkItemStatus), item.Status))
                    {
                        throw new InvalidOperationException($"Invalid WorkItem.Status: {item.Status}");
                    }

                    if (!string.IsNullOrEmpty(item.Assignee) && item.Assignee.Length > 256)
                    {
                        throw new InvalidOperationException("WorkItem.Assignee exceeds 256 character limit");
                    }
                }
            }
        }

        private string HashFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                using (var sha256 = SHA256.Create())
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        var hash = sha256.ComputeHash(fileStream);
                        return Convert.ToBase64String(hash);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to compute file hash: {ex.Message}");
                return null;
            }
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

            return _cachedData.WorkItems.AsReadOnly();
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

            var shipped = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.Shipped);
            var inProgress = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.InProgress);
            var carriedOver = _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.CarriedOver);

            return (shipped, inProgress, carriedOver);
        }

        public string GetLastError()
        {
            return _lastError;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _debounceTimer?.Dispose();
            _watcher?.Dispose();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}