using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class DashboardDataService
{
    private readonly string _dataFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public DashboardDataService(string dataFilePath)
    {
        _dataFilePath = dataFilePath;
    }

    /// <summary>
    /// Reads and deserializes data.json on every call with no caching,
    /// so PM edits are reflected on browser refresh without app restart.
    /// </summary>
    public async Task<DashboardData> GetDashboardDataAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            throw new FileNotFoundException(
                $"Dashboard data file not found at: {_dataFilePath}");
        }

        await using var stream = File.OpenRead(_dataFilePath);
        var data = await JsonSerializer.DeserializeAsync<DashboardData>(stream, JsonOptions);

        return data ?? throw new InvalidOperationException(
            "Failed to deserialize dashboard data: result was null.");
    }

    public DashboardData GetDashboardData()
    {
        if (!File.Exists(_dataFilePath))
        {
            throw new FileNotFoundException(
                $"Dashboard data file not found at: {_dataFilePath}");
        }

        var json = File.ReadAllText(_dataFilePath);
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        return data ?? throw new InvalidOperationException(
            "Failed to deserialize dashboard data: result was null.");
    }
}