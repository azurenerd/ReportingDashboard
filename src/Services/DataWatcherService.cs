using System;
using System.IO;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
    public class DataWatcherService : IDataWatcherService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataWatcherService> _logger;

        private FileSystemWatcher _fileWatcher;
        private Timer _debounceTimer;
        private string _dataPath;
        private int _debounceIntervalMs;
        private bool _isDisposed;
        private SynchronizationContext _blazorSynchronizationContext;

        public event Func<Task> OnDataChanged;

        public DateTime LastRefreshTime { get; private set; }

        public string LastRefreshTimeFormatted => LastRefreshTime.ToString("HH:mm:ss");

        public DataWatcherService(IConfiguration configuration, ILogger<DataWatcherService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LastRefreshTime = DateTime.Now;
            _isDisposed = false;
            _blazorSynchronizationContext = SynchronizationContext.Current;
        }

        public void Start(string dataPath = null)
        {
            if (_isDisposed)
            {
                _logger.LogWarning("Cannot start DataWatcherService: service has been disposed.");
                return;
            }

            try
            {
                _dataPath = ResolveDataPath(dataPath);
                _debounceIntervalMs = GetDebounceInterval();

                ValidateDataPath(_dataPath);

                InitializeFileSystemWatcher();
                InitializeDebounceTimer();

                _logger.LogInformation($"DataWatcherService started monitoring: {_dataPath}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to start DataWatcherService: {ex.Message}. File monitoring will be unavailable.");
                CleanupResources();
            }
        }

        public void Stop()
        {
            CleanupResources();
            _logger.LogInformation("DataWatcherService stopped.");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            Dispose(true);
            await ValueTask.CompletedTask;
            GC.SuppressFinalize(this);
        }

        private string ResolveDataPath(string dataPath)
        {
            if (!string.IsNullOrWhiteSpace(dataPath))
            {
                return dataPath;
            }

            string configPath = _configuration["AppSettings:DataPath"];
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                return configPath;
            }

            return "data.json";
        }

        private int GetDebounceInterval()
        {
            string debounceConfig = _configuration["AppSettings:DebounceIntervalMs"];
            if (int.TryParse(debounceConfig, out int interval) && interval > 0)
            {
                return interval;
            }

            return 500;
        }

        private void ValidateDataPath(string dataPath)
        {
            if (string.IsNullOrWhiteSpace(dataPath))
            {
                throw new ArgumentException("Data path cannot be null or empty.", nameof(dataPath));
            }

            string fullPath = Path.GetFullPath(dataPath);
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Directory does not exist: {directory}");
            }
        }

        private void InitializeFileSystemWatcher()
        {
            string fullPath = Path.GetFullPath(_dataPath);
            string directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(fullPath);

            _fileWatcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.Error += OnFileWatcherError;
        }

        private void InitializeDebounceTimer()
        {
            _debounceTimer = new Timer(_debounceIntervalMs)
            {
                AutoReset = false,
                Enabled = false
            };

            _debounceTimer.Elapsed += OnDebounceTimerElapsed;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            _logger.LogInformation($"Data file change detected at {DateTime.Now:HH:mm:ss.fff}");

            lock (_debounceTimer)
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
        }

        private void OnDebounceTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_debounceTimer)
            {
                _debounceTimer.Stop();
            }

            LastRefreshTime = DateTime.Now;
            _logger.LogInformation($"Data refresh triggered at {LastRefreshTimeFormatted}");

            FireDataChangedEvent();
        }

        private void FireDataChangedEvent()
        {
            if (OnDataChanged == null)
            {
                return;
            }

            if (_blazorSynchronizationContext != null)
            {
                _blazorSynchronizationContext.Post(async _ =>
                {
                    try
                    {
                        await OnDataChanged.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in OnDataChanged event handler: {ex.Message}");
                    }
                }, null);
            }
            else
            {
                try
                {
                    var task = OnDataChanged.Invoke();
                    if (task != null)
                    {
                        task.GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in OnDataChanged event handler (no SynchronizationContext): {ex.Message}");
                }
            }
        }

        private void OnFileWatcherError(object sender, ErrorEventArgs e)
        {
            Exception exception = e.GetException();
            if (exception != null)
            {
                _logger.LogWarning($"FileSystemWatcher error: {exception.Message}");
            }
        }

        private void CleanupResources()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Changed -= OnFileChanged;
                _fileWatcher.Error -= OnFileWatcherError;
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }

            if (_debounceTimer != null)
            {
                _debounceTimer.Stop();
                _debounceTimer.Elapsed -= OnDebounceTimerElapsed;
                _debounceTimer.Dispose();
                _debounceTimer = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                CleanupResources();
            }

            _isDisposed = true;
        }
    }
}