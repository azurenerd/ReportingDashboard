using AgentSquad.Dashboard.Services;
using Microsoft.AspNetCore.SignalR;

namespace AgentSquad.Dashboard.Hubs;

public class AgentHub : Hub
{
    private readonly IDashboardDataService _dataService;
    private readonly ILogger<AgentHub> _logger;

    public AgentHub(IDashboardDataService dataService, ILogger<AgentHub> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        
        var data = _dataService.CurrentData;
        await Clients.Caller.SendAsync("ReceiveDashboardData", data);
        
        await base.OnConnectedAsync();
    }

    public async Task RequestDashboardUpdate()
    {
        await Clients.All.SendAsync("ReceiveDashboardData", _dataService.CurrentData);
    }

    public async Task BroadcastDashboardRefresh()
    {
        _logger.LogInformation("Broadcasting dashboard refresh");
        await Clients.All.SendAsync("RefreshDashboard");
    }
}