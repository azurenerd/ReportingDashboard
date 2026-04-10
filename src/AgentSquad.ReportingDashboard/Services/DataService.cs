using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using AgentSquad.ReportingDashboard.Models;

namespace AgentSquad.ReportingDashboard.Services
{
    public class DataService : IAsyncDisposable
    {
        private FileSystemWatcher _fileWatcher;
        private CancellationTokenSource _debounceCts;
        private string _dataFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public DashboardData CurrentData { get; private set; }

        public event Func<Task> OnDataChanged;

        public DataService(IConfiguration configuration)
        {
            _dataFilePath = configuration["Dashboard:DataFilePath"] ?? "wwwroot/data.json";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            CurrentData = new DashboardData
            {
                ProjectName = "Dashboard",
                ProjectStatus = "Unknown",
                Milestones = new List<Milestone>(),
                ShippedItems = new List<MetricItem>(),
                InProgressItems = new List<MetricItem>(),
                CarryoverItems = new List<MetricItem>(),
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task InitializeAsync()
        {
            try
            {
                var json = File.ReadAllText(_dataFilePath);
                var loadedData = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);

                if (loadedData != null)
                {
                    CurrentData = loadedData;
                }
                else
                {
                    Debug.WriteLine("DataService: Deserialized data was null; using defaults");
                    SetDefaultData();
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine($"DataService: data.json not found at '{_dataFilePath}'; using defaults");
                SetDefaultData();
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"DataService: JSON parse error - {ex.Message}; using defaults");
                SetDefaultData();
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"DataService: File access error - {ex.Message}; using defaults");
                SetDefaultData();
            }

            SetupFileSystemWatcher();
        }

        private void SetupFileSystemWatcher()
        {
            try
            {
                string watchPath = Path.GetDirectoryName(_dataFilePath);
                if (string.IsNullOrEmpty(watchPath))
                {
                    watchPath = ".";
                }

                _fileWatcher = new FileSystemWatcher(watchPath)
                {
                    Filter = "data.json",
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                _fileWatcher.Created += OnFileChanged;
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Renamed += OnFileChanged;

                _fileWatcher.EnableRaisingEvents = true;

                Debug.WriteLine($"DataService: FileSystemWatcher initialized for '{watchPath}\\data.json'");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DataService: Failed to setup FileSystemWatcher - {ex.Message}");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("DataService: FileSystemWatcher detected data.json change");
            _ = DebouncedReload();
        }

        private async Task DebouncedReload()
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();

            _debounceCts = new CancellationTokenSource();
            var cts = _debounceCts;

            try
            {
                await Task.Delay(500, cts.Token);
                await ReloadDataAsync();
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("DataService: Debounce cancelled due to rapid successive file changes");
            }
        }

        public async Task<DashboardData> ReloadDataAsync()
        {
            return CurrentData;
        }

        private void SetDefaultData()
        {
            CurrentData = new DashboardData
            {
                ProjectName = "Dashboard",
                ProjectStatus = "Unknown",
                Milestones = new List<Milestone>(),
                ShippedItems = new List<MetricItem>(),
                InProgressItems = new List<MetricItem>(),
                CarryoverItems = new List<MetricItem>(),
                LastUpdated = DateTime.UtcNow
            };
        }

        public async ValueTask DisposeAsync()
        {
            Dispose();
        }

        public void Dispose()
        {
            _fileWatcher?.EnableRaisingEvents = false;
            _fileWatcher?.Dispose();
            _debounceCts?.Dispose();
        }
    }
}