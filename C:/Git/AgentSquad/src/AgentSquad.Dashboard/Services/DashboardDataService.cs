using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Services;

public class DashboardDataService : BackgroundService
{
    private readonly ProjectDataService _projectDataService;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IHubContext<AgentHub> _hubContext;
    private ProjectData? _currentData;

    public DashboardDataService(
        ProjectDataService projectDataService,
        ILogger<DashboardDataService> logger,
        IHubContext<AgentHub> hubContext)
    {
        _projectDataService = projectDataService ?? throw new ArgumentNullException(nameof(projectDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public ProjectData? GetCurrentData() => _currentData;

    public async Task<ProjectData> RefreshDataAsync()
    {
        try
        {
            _currentData = await _projectDataService.LoadProjectDataAsync();
            return _currentData;
        }
        catch (DataLoadException ex)
        {
            _logger.LogError(ex, "Failed to load project data");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _currentData = await _projectDataService.LoadProjectDataAsync();
            _logger.LogInformation("DashboardDataService initialized with project data");
        }
        catch (DataLoadException ex)
        {
            _logger.LogError(ex, "Failed to initialize DashboardDataService");
        }
    }
}

public interface IAgentHub
{
    Task ReceiveDataUpdate(ProjectData data);
}

public class AgentHub : Hub<IAgentHub>
{
}