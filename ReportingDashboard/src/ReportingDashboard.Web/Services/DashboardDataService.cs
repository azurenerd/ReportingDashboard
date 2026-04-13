using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class DashboardDataService : IDashboardDataService
{
    private readonly IWebHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Reads and deserializes data.json on every call with no caching,
    /// so PM edits are reflected on browser refresh without app restart.
    /// </summary>
    public async Task<DashboardData> GetDashboardDataAsync()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "Data", "data.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Dashboard data file not found at: {filePath}. Ensure Data/data.json exists in the project root.");
        }

        await using var stream = File.OpenRead(filePath);
        var data = await JsonSerializer.DeserializeAsync<DashboardData>(stream, JsonOptions);

        return data ?? throw new InvalidOperationException(
            "Failed to deserialize data.json. The file may be empty or contain invalid JSON structure.");
    }
}