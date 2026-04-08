using Microsoft.AspNetCore.SignalR;

namespace AgentSquad.Dashboard.Hubs;

public class AgentHub : Hub
{
    private readonly ILogger<AgentHub> _logger;

    public AgentHub(ILogger<AgentHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ClientId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation("Client disconnected: {ClientId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}