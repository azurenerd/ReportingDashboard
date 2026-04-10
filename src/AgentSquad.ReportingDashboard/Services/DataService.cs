using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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