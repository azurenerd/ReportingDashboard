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

        public event Func<Task> OnDataChanged;

        public DateTime LastRefreshTime { get; private set; }

        public string LastRefreshTimeFormatted => LastRefreshTime.ToString("HH:mm:ss");

        public DataWatcherService(IConfiguration configuration, ILogger<DataWatcherService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LastRefreshTime = DateTime.Now;
        }

        public void Start(string dataPath = null)
        {
            throw new NotImplementedException("Start method is not yet implemented.");
        }

        public void Stop()
        {
            throw new NotImplementedException("Stop method is not yet implemented.");
        }

        public void Dispose()
        {
            throw new NotImplementedException("Dispose method is not yet implemented.");
        }

        public async ValueTask DisposeAsync()
        {
            throw new NotImplementedException("DisposeAsync method is not yet implemented.");
        }
    }
}