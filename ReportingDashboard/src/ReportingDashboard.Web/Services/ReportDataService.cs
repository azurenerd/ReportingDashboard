using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class ReportDataService : IReportDataService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ReportDataService> _logger;

    public ReportDataService(IWebHostEnvironment environment, ILogger<ReportDataService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<ProjectReport> GetReportAsync()
    {
        var filePath = Path.Combine(_environment.WebRootPath, "data", "data.json");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("data.json not found at {Path}. Returning empty report.", filePath);
            return new ProjectReport { ProjectName = "No Data Loaded" };
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var report = await JsonSerializer.DeserializeAsync<ProjectReport>(stream);
            return report ?? new ProjectReport { ProjectName = "Failed to Parse Data" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading data.json from {Path}", filePath);
            return new ProjectReport { ProjectName = "Error Loading Data" };
        }
    }
}