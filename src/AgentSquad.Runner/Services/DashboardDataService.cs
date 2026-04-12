using AgentSquad.Runner.Models;
using System.Text.Json;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service for loading and managing dashboard configuration from data.json file.
    /// Stub implementation: complete implementation deferred to subsequent PR.
    /// </summary>
    public class DashboardDataService : IDashboardDataService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DashboardDataService> _logger;

        public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Load and return parsed dashboard configuration from data.json file.
        /// Stub: implementation deferred.
        /// </summary>
        public async Task<DashboardConfig> GetDashboardConfigAsync()
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Refresh configuration by re-reading data.json from disk.
        /// Stub: implementation deferred.
        /// </summary>
        public async Task RefreshAsync()
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Get the file system modification time of data.json.
        /// Stub: implementation deferred.
        /// </summary>
        public DateTime GetLastModifiedTime()
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }
    }
}