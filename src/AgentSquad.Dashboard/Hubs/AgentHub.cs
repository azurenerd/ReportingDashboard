using Microsoft.AspNetCore.SignalR;

namespace AgentSquad.Dashboard.Hubs;

public class AgentHub : Hub
{
    private readonly ILogger<AgentHub> _logger;

    public AgentHub(ILogger<AgentHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendAgentUpdate(string message)
    {
        _logger.LogDebug("Broadcasting agent update: {Message}", message);
        await Clients.All.SendAsync("ReceiveUpdate", message);
    }

    public async Task SendTaskUpdate(string taskId, string status)
    {
        _logger.LogDebug("Broadcasting task update: {TaskId} -> {Status}", taskId, status);
        await Clients.All.SendAsync("ReceiveTaskUpdate", taskId, status);
    }
}