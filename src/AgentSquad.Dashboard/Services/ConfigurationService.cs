using AgentSquad.Core.Configuration;
using Microsoft.Extensions.Options;

namespace AgentSquad.Dashboard.Services;

public class ConfigurationService
{
    private readonly IOptions<AgentSquadConfig> _config;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(
        IOptions<AgentSquadConfig> config,
        ILogger<ConfigurationService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public AgentSquadConfig GetConfiguration()
    {
        return _config.Value;
    }

    public int GetDashboardPort()
    {
        return _config.Value.Dashboard.Port;
    }

    public bool IsSignalREnabled()
    {
        return _config.Value.Dashboard.EnableSignalR;
    }

    public string GetProjectName()
    {
        return _config.Value.Project.Name;
    }

    public string GetProjectDescription()
    {
        return _config.Value.Project.Description;
    }
}