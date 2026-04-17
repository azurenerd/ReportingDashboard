using System.Text.Json;

namespace ReportingDashboard.Data;

public class DashboardDataService
{
    private readonly string _filePath;

    public DashboardDataService(IConfiguration config)
    {
        _filePath = config.GetValue<string>("DashboardDataPath") ?? "Data/data.json";
    }

    public async Task<DashboardReport> GetDataAsync()
    {
        var fullPath = Path.GetFullPath(_filePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Dashboard data file not found at: {fullPath}");
        }

        var json = await File.ReadAllTextAsync(fullPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var result = JsonSerializer.Deserialize<DashboardReport>(json, options);
        if (result == null)
        {
            throw new InvalidOperationException("data.json deserialized to null");
        }

        return result;
    }
}