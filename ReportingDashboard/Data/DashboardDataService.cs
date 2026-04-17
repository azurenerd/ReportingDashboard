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
        if (!File.Exists(_filePath))
            throw new FileNotFoundException(
                $"Dashboard data file not found at: {Path.GetFullPath(_filePath)}");

        var json = await File.ReadAllTextAsync(_filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<DashboardReport>(json, options)
            ?? throw new InvalidOperationException("data.json deserialized to null.");
    }
}