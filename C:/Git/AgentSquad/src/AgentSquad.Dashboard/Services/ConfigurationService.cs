using AgentSquad.Dashboard.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Dashboard.Services;

public class ConfigurationService
{
    private readonly IDashboardDataService _dataService;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(IDashboardDataService dataService, ILogger<ConfigurationService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public DashboardData GetCurrentConfiguration() => _dataService.CurrentData;

    public bool ValidateConfiguration() => _dataService.IsValid;

    public string? GetLastError() => _dataService.LastErrorMessage;
}