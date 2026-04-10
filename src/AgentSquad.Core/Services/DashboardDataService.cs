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
using AgentSquad.Core.Models;

namespace AgentSquad.Core.Services
{
    public class DashboardDataService : IDisposable
    {
        private readonly ILogger<DashboardDataService> _logger;
        private readonly IOptions<DashboardOptions> _options;
        private readonly string _dataJsonPath;
        private readonly int _debounceMs;
        private DashboardData _cachedData;
        private string _lastError;
        private string _lastLoadedHash;
        private FileSystemWatcher _fileWatcher;
        private Timer _debounceTimer;

        public event Action OnDataChanged;

        public bool HasData => _cachedData != null && string.IsNullOrEmpty(_lastError);

        public DashboardDataService(ILogger<DashboardDataService> logger, IOptions<DashboardOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataJsonPath = options.Value.DataJsonPath;
            _debounceMs = options.Value.FileWatchDebounceMs;

            LoadInitialData();
            InitializeFileWatcher();
        }

        private void LoadInitialData()
        {
            try
            {
                if (!File.Exists(_dataJsonPath))
                {
                    _lastError = $"data.json not found at {_dataJsonPath}";
                    _logger.LogWarning(_lastError);
                    return;
                }

                var json = File.ReadAllText(_dataJsonPath, Encoding.UTF8);
                ParseAndCacheData(json);
                _logger.LogInformation("DashboardDataService initialized, data.json loaded successfully");
            }
            catch (Exception ex)
            {
                _lastError = $"Failed to load data.json: {ex.Message}";
                _logger.LogError(_lastError);
            }
        }

        private void InitializeFileWatcher()
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataJsonPath);
                var fileName = Path.GetFileName(_dataJsonPath);

                _fileWatcher = new FileSystemWatcher(directory, fileName)
                {
                    EnableRaisingEvents = true
                };

                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Created += OnFileChanged;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to initialize FileSystemWatcher: {ex.Message}");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ => ReloadData(), null, _debounceMs, Timeout.Infinite);
        }

        private void ReloadData()
        {
            try
            {
                if (!File.Exists(_dataJsonPath))
                {
                    _lastError = $"data.json not found at {_dataJsonPath}";
                    _cachedData = null;
                    OnDataChanged?.Invoke();
                    return;
                }

                var json = ReadFileWithRetry(_dataJsonPath, 3);
                var fileHash = ComputeHash(json);

                if (fileHash == _lastLoadedHash)
                {
                    _logger.LogDebug("File content unchanged; skipping reload");
                    return;
                }

                ParseAndCacheData(json);
                _logger.LogInformation("data.json reloaded successfully");
                OnDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _lastError = $"Error reloading data.json: {ex.Message}";
                _logger.LogError(_lastError);
                OnDataChanged?.Invoke();
            }
        }

        private void ParseAndCacheData(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = false,
                    MaxDepth = 64
                };

                var data = JsonSerializer.Deserialize<DashboardData>(json, options);

                if (data?.Project == null)
                    throw new JsonException("Project is required");

                _cachedData = data;
                _lastLoadedHash = ComputeHash(json);
                _lastError = null;
            }
            catch (JsonException ex)
            {
                _lastError = $"JSON parsing error: {ex.Message}";
                _logger.LogError(_lastError);
                throw;
            }
        }

        private string ReadFileWithRetry(string path, int maxRetries)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return File.ReadAllText(path, Encoding.UTF8);
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    _logger.LogDebug($"File locked; retrying ({i + 1}/{maxRetries})");
                    Thread.Sleep(100);
                }
            }

            throw new IOException($"Could not read {path} after {maxRetries} retries");
        }

        private string ComputeHash(string content)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
                return Convert.ToBase64String(bytes);
            }
        }

        public DashboardData GetCurrentData() => _cachedData;

        public Project GetProject() => _cachedData?.Project;

        public IReadOnlyList<Milestone> GetMilestones()
        {
            if (_cachedData?.Milestones == null)
                return new List<Milestone>();

            return _cachedData.Milestones
                .OrderBy(m => m.Date)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<WorkItem> GetWorkItems()
        {
            if (_cachedData?.WorkItems == null)
                return new List<WorkItem>();

            return _cachedData.WorkItems.AsReadOnly();
        }

        public IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status)
        {
            if (_cachedData?.WorkItems == null)
                return new List<WorkItem>();

            return _cachedData.WorkItems
                .Where(w => w.Status == status)
                .ToList()
                .AsReadOnly();
        }

        public (int Shipped, int InProgress, int CarriedOver) GetStatusCounts()
        {
            if (_cachedData?.WorkItems == null)
                return (0, 0, 0);

            return (
                _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.Shipped),
                _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.InProgress),
                _cachedData.WorkItems.Count(w => w.Status == WorkItemStatus.CarriedOver)
            );
        }

        public string GetLastError() => _lastError;

        public void Dispose()
        {
            _debounceTimer?.Dispose();
            _fileWatcher?.Dispose();
        }
    }
}