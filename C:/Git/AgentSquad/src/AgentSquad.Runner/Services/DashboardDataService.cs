namespace AgentSquad.Dashboard.Services;

public class DashboardDataService : BackgroundService
{
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataService(ILogger<DashboardDataService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DashboardDataService started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("DashboardDataService stopped");
    }
}