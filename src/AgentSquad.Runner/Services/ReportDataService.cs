using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class ReportDataService
    {
        private ProjectReport? _cachedReport;
        private readonly ILogger<ReportDataService> _logger;

        public bool IsError { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        public ReportDataService(ILogger<ReportDataService> logger)
        {
            _logger = logger;
        }

        public ProjectReport? GetReport()
        {
            if (_cachedReport != null && !IsError)
                return _cachedReport;

            try
            {
                IsError = false;
                ErrorMessage = string.Empty;

                var filePath = Path.Combine(AppContext.BaseDirectory, "data.json");
                
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"data.json not found at {filePath}");
                }

                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _cachedReport = JsonSerializer.Deserialize<ProjectReport>(json, options);

                if (_cachedReport == null)
                {
                    throw new JsonException("Failed to deserialize ProjectReport from data.json");
                }

                _logger.LogInformation("Report data loaded successfully from {FilePath}", filePath);
                return _cachedReport;
            }
            catch (FileNotFoundException ex)
            {
                IsError = true;
                ErrorMessage = $"data.json not found. {ex.Message}";
                _logger.LogError("File not found: {Message}", ex.Message);
                return null;
            }
            catch (JsonException ex)
            {
                IsError = true;
                ErrorMessage = $"Invalid JSON in data.json. Please check file syntax. Details: {ex.Message}";
                _logger.LogError("JSON parsing error: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = $"Unexpected error loading dashboard data: {ex.Message}";
                _logger.LogError("Unexpected error loading report data: {Message}", ex.Message);
                return null;
            }
        }
    }
}