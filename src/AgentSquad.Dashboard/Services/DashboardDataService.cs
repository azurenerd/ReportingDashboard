using AgentSquad.Core.Messaging;

namespace AgentSquad.Dashboard.Services;

public class DashboardDataService : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly ProjectDashboardState _dashboardState;

    public DashboardDataService(
        IMessageBus messageBus,
        ILogger<DashboardDataService> logger,
        ProjectDashboardState dashboardState)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dashboardState = dashboardState ?? throw new ArgumentNullException(nameof(dashboardState));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DashboardDataService started");

        try
        {
            await _dashboardState.InitializeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize dashboard state");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("DashboardDataService stopped");
    }
}