using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgentSquad.Runner.Exceptions;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Implementation of IDashboardDataService that reads from data.json file
    /// </summary>
    public class DashboardDataService : IDashboardDataService
    {
        private readonly ILogger<DashboardDataService> _logger;
        private readonly IConfiguration _config;
        private DashboardModel? _cachedData;

        public DashboardDataService(ILogger<DashboardDataService> logger, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<DashboardModel> GetDashboardDataAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedData != null)
                return _cachedData;

            string dataPath = _config.GetValue<string>("DataFilePath", "./wwwroot/data/data.json") 
                ?? "./wwwroot/data/data.json";

            try
            {
                _logger.LogInformation("Loading dashboard data from {Path}", dataPath);
                
                string json = await File.ReadAllTextAsync(dataPath, cancellationToken);
                
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };
                
                _cachedData = JsonSerializer.Deserialize<DashboardModel>(json, options)
                    ?? throw new DashboardDataException("Deserialized data is null");
                
                DashboardModelValidator.Validate(_cachedData);
                
                _logger.LogInformation("Dashboard data loaded successfully from {Path}", dataPath);
                return _cachedData;
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Data file not found at {Path}", dataPath);
                throw new DashboardDataException(
                    "Unable to load dashboard data. Please check that data.json is available and properly formatted.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in data file at {Path}", dataPath);
                throw new DashboardDataException(
                    "Unable to load dashboard data. Please check that data.json is properly formatted.", ex);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Data validation failed for {Path}: {Message}", dataPath, ex.Message);
                throw new DashboardDataException(
                    "Unable to load dashboard data. Please check that data.json contains all required fields.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading dashboard data from {Path}", dataPath);
                throw new DashboardDataException(
                    "Unable to load dashboard data. An unexpected error occurred.", ex);
            }
        }

        public async Task ReloadDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Reloading dashboard data");
            _cachedData = null;
            await GetDashboardDataAsync(cancellationToken);
        }
    }
}