using System.Security.Cryptography;
using System.Text.Json;
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
    }

    public DashboardData GetCurrentData()
    {
        throw new NotImplementedException();
    }

    public Project GetProject()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<Milestone> GetMilestones()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<WorkItem> GetWorkItems()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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