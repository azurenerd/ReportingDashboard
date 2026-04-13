using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private DashboardData _data = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public DashboardData Data => _data;

    public async Task LoadDataAsync()
    {
        var path = Path.Combine(_env.WebRootPath, "data", "data.json");
        if (File.Exists(path))
        {
            var json = await File.ReadAllTextAsync(path);
            _data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions) ?? new DashboardData();
        }
    }

    public List<WorkItem> GetShippedItems() =>
        _data.WorkItems.Where(w => w.Category == "Shipped").ToList();

    public List<WorkItem> GetInProgressItems() =>
        _data.WorkItems.Where(w => w.Category == "InProgress").ToList();

    public List<WorkItem> GetCarriedOverItems() =>
        _data.WorkItems.Where(w => w.Category == "CarriedOver").ToList();
}