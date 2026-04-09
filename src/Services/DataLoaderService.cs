using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
    public class DataLoaderService : IDataLoaderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataLoaderService> _logger;

        public DataLoaderService(IConfiguration configuration, ILogger<DataLoaderService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetConfiguredDataPath()
        {
            string configPath = _configuration.GetValue<string>("AppSettings:DataPath");
            if (!string.IsNullOrEmpty(configPath))
            {
                return Path.GetFullPath(configPath);
            }
            return Path.GetFullPath("data.json");
        }

        public async Task<ProjectReport> LoadAsync(string dataPath = null)
        {
            try
            {
                string resolvedPath = dataPath;
                if (string.IsNullOrEmpty(resolvedPath))
                {
                    resolvedPath = GetConfiguredDataPath();
                }
                else
                {
                    resolvedPath = Path.GetFullPath(resolvedPath);
                }

                _logger.LogInformation($"Loading data from: {resolvedPath}");

                if (!File.Exists(resolvedPath))
                {
                    throw new FileNotFoundException($"Data file not found at {resolvedPath}");
                }

                string jsonContent = await File.ReadAllTextAsync(resolvedPath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var report = JsonSerializer.Deserialize<ProjectReport>(jsonContent, options);

                _logger.LogInformation($"Successfully loaded project report: {report?.ProjectName ?? "Unknown"} with {report?.Milestones?.Length ?? 0} milestones");

                return report;
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError($"File not found: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                throw;
            }
            catch (IOException ex)
            {
                _logger.LogError($"IO error reading data file: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error loading data: {ex.Message}");
                throw;
            }
        }
    }
}