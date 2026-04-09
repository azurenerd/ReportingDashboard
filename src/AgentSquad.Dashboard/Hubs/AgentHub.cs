using Microsoft.AspNetCore.SignalR;

namespace AgentSquad.Dashboard.Hubs;

public class AgentHub : Hub
{
    public async Task SendDashboardUpdate(object data)
    {
        await Clients.All.SendAsync("ReceiveDashboardUpdate", data);
    }
}