using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AgentSquad.Runner.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
    public class DataWatcherService : IDataWatcherService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataWatcherService> _logger;
        private FileSystemWatcher _fileWatcher;
        private System.Timers.Timer _debounceTimer;
        private DateTime _lastRefreshTime = DateTime.Now;
        private bool _disposed = false;

        public event Func<Task> OnDataChanged;

        public DateTime LastRefreshTime => _lastRefreshTime;

        public string LastRefreshTimeFormatted => _lastRefreshTime.ToString("HH:mm:ss");

        public DataWatcherService(IConfiguration configuration, ILogger<DataWatcherService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start(string dataPath = null)
        {
            try
            {
                if (_fileWatcher != null)
                {
                    _logger.LogWarning("DataWatcherService already started; Stop() before calling Start() again");
                    return;
                }

                string resolvedPath = ResolvePath(dataPath);
                if (string.IsNullOrEmpty(resolvedPath))
                {
                    _logger.LogWarning("DataWatcherService: data path is null or empty; watcher not started");
                    return;
                }

                string directoryPath = Path.GetDirectoryName(resolvedPath);
                string fileName = Path.GetFileName(resolvedPath);

                if (string.IsNullOrEmpty(directoryPath))
                {
                    directoryPath = ".";
                }

                if (!Directory.Exists(directoryPath))
                {
                    _logger.LogWarning($"DataWatcherService: directory does not exist at {directoryPath}; watcher not started");
                    return;
                }

                _fileWatcher = new FileSystemWatcher(directoryPath)
                {
                    Filter = fileName,
                    NotifyFilter = NotifyFilters.LastWrite
                };

                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;

                int debounceMs = _configuration.GetValue("AppSettings:DebounceIntervalMs", 500);
                _debounceTimer = new System.Timers.Timer(debounceMs)
                {
                    AutoReset = false
                };
                _debounceTimer.Elapsed += async (s, e) => await FireDataChangedEvent();

                _logger.LogInformation($"DataWatcherService started monitoring: {resolvedPath}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"DataWatcherService failed to start: {ex.Message}");
                // Graceful degradation: continue without watcher
            }
        }

        public void Stop()
        {
            try
            {
                if (_fileWatcher != null)
                {
                    _fileWatcher.EnableRaisingEvents = false;
                    _fileWatcher.Dispose();
                    _fileWatcher = null;
                    _logger.LogInformation("DataWatcherService stopped monitoring");
                }

                if (_debounceTimer != null)
                {
                    _debounceTimer.Stop();
                    _debounceTimer.Dispose();
                    _debounceTimer = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"DataWatcherService error during Stop: {ex.Message}");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (_debounceTimer == null)
                {
                    return;
                }

                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"DataWatcherService error in OnFileChanged: {ex.Message}");
            }
        }

        private async Task FireDataChangedEvent()
        {
            try
            {
                _lastRefreshTime = DateTime.Now;
                _logger.LogInformation($"DataWatcherService: triggering OnDataChanged at {LastRefreshTimeFormatted}");

                if (OnDataChanged != null)
                {
                    await OnDataChanged.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"DataWatcherService error in FireDataChangedEvent: {ex.Message}");
            }
        }

        private string ResolvePath(string dataPath)
        {
            if (!string.IsNullOrEmpty(dataPath))
            {
                return Path.GetFullPath(dataPath);
            }

            string configPath = _configuration.GetValue<string>("AppSettings:DataPath");
            if (!string.IsNullOrEmpty(configPath))
            {
                return Path.GetFullPath(configPath);
            }

            return Path.GetFullPath("data.json");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Stop();
            }

            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                Stop();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"DataWatcherService error during DisposeAsync: {ex.Message}");
            }

            Dispose(false);
            GC.SuppressFinalize(this);
        }

        ~DataWatcherService()
        {
            Dispose(false);
        }
    }
}