using ReportingDashboard.Data;

namespace ReportingDashboard.Data;

public class DashboardDataService
{
    private readonly string _filePath;

    public DashboardDataService(IConfiguration config)
    {
        _filePath = config.GetValue<string>("DashboardDataPath") ?? "Data/data.json";
    }

    public async Task<(DashboardReport? Data, string? ErrorMessage)> GetDataAsync()
    {
        // Stub: T7 fills in the implementation body
        await Task.CompletedTask;
        return (null, "Not implemented");
    }
}